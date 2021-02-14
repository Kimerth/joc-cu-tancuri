using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

class PhysicsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<Collider>().ForEach((ref Translation trans, ref Rotation rot, ref Collider collider) =>
        {
            float angle = math.degrees(Quaternion.ToEulerAngles(rot.Value).y);
            
            float x = collider.size.x / 2;
            float y = collider.size.y / 2;
            float2 offset = new float2(x * math.cos(angle) - y * math.sin(angle), x * math.sin(angle) + y * math.cos(angle));

            collider.vertices.c0 = toFloat2(trans.Value) + collider.origin - offset;
            collider.vertices.c1 = toFloat2(trans.Value) + collider.origin + new float2(-offset.x, offset.y);
            collider.vertices.c2 = toFloat2(trans.Value) + collider.origin + offset;
            collider.vertices.c3 = toFloat2(trans.Value) + collider.origin + new float2(offset.x, -offset.y);

            for (int i = 0; i < 4; i++)
            {
                float2 aux = collider.vertices[(i + 1) % 4] - collider.vertices[i];
                //float normal_angl = -1 / math.tan(math.atan2(aux.x, aux.y));
                //collider.normals[i] = new float2(math.cos(normal_angl), math.sin(normal_angl));
                collider.normals[i] = math.normalize(new float2(-aux.y, aux.x));
            }
        });

        Entities.WithAll<Rigidbody>().ForEach((Entity entity, ref Translation trans, ref Rigidbody rigidbody, ref Collider collider) =>
        {
            float dt = UnityEngine.Time.deltaTime;
            float2 displacement = rigidbody.velocity * dt;

            if (rigidbody.velocity.x != 0f || rigidbody.velocity.y != 0f)
            {
                int steps = math.max(1, (int)math.round(math.length(displacement) / 0.05f));
                float2 move_step = displacement / steps;
                float2 transform = toFloat2(trans.Value);
                var colliderQuery = Entities.WithAll<Collider>();
                var collider_copy = collider;
                for (int s = 0; s < steps; s++)
                {
                    transform += move_step;
                    bool collided = false;
                    float2 normal = float2.zero;
                    colliderQuery.ForEach((Entity other_entity, ref Translation other_trans, ref Collider other_collider) =>
                    {
                        if(entity != other_entity && !collided)
                            collided = CheckCollision(collider_copy, other_collider, out normal);
                    });
                    
                    if (collided)
                    {
                        float2 normal_projection = math.project(rigidbody.velocity, normal);

                        rigidbody.velocity -= normal_projection * 2 * collider.bounciness;

                        trans.Value += toFloat3(rigidbody.velocity) * dt;

                        break;
                    }
                    else
                        trans.Value = toFloat3(transform);
                }
            }

            rigidbody.velocity += rigidbody.acceleration;
            rigidbody.velocity -= math.normalize(rigidbody.velocity) * rigidbody.drag;
        });
    }

    float2 GetSupport(in float2 dir, in float2x4 vertices)
    {
        float bestProjection = -float.MaxValue;
        float2 bestVertex = float2.zero;
 
        for(int i = 0; i<4; i++)
        {
            float2 v = vertices[i];
            float projection = math.dot(v, dir);
 
            if(projection > bestProjection)
            {
                bestVertex = v;
                bestProjection = projection;
            }
        }
 
        return bestVertex;
    }

    float FindAxisLeastPenetration(in Collider A, in Collider B, out int faceIndex)
    {
        float bestDistance = -float.MaxValue;
        faceIndex = 0;

        for (int i = 0; i < 4; i++)
        {
            float2 n = A.normals[i];
            float2 s = GetSupport(-n, B.vertices);
            float2 v = A.vertices[i];
            float d = math.dot(n, s - v);

            if (d > bestDistance)
            {
                bestDistance = d;
                faceIndex = i;
            }
        }

        return bestDistance;
    }

    bool CheckCollision(in Collider A, in Collider B, out float2 normal)
    {
        int idx;
        if(FindAxisLeastPenetration(A, B, out idx) < 0)
        {
            normal = A.normals[idx];
            return true;
        }
        else if(FindAxisLeastPenetration(B, A, out idx) < 0)
        {
            normal = B.normals[idx];
            return true;
        }

        normal = float2.zero;
        return false;
    }

    static float2 toFloat2(float3 v1)
    {
        float2 v2;
        v2.x = v1.x;
        v2.y = v1.z;
        return v2;
    }

    static float3 toFloat3(float2 v1)
    {
        float3 v2;
        v2.x = v1.x;
        v2.y = 0;
        v2.z = v1.y;
        return v2;
    }
}
