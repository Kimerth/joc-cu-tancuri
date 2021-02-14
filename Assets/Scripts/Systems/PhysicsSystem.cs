using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

class PhysicsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
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
                    Rect collider_rect = new Rect(transform.x + collider.origin.x, transform.y + collider.origin.y, collider.size.x, collider.size.y);
                    colliderQuery.ForEach((Entity other_entity, ref Translation other_trans, ref Collider other_collider) =>
                    {
                        if(entity != other_entity)
                        {
                            Rect other_collider_rect = new Rect(
                                other_trans.Value.x + other_collider.origin.x, 
                                other_trans.Value.y + other_collider.origin.y, 
                                other_collider.size.x, 
                                other_collider.size.y
                            );
                            collided = collided || collider_rect.Overlaps(other_collider_rect);
                        }
                    });

                    if (collided)
                    {
                        rigidbody.velocity = new float2(0, 0);
                        break;
                    }
                    else
                        trans.Value = toFloat3(transform);
                }
            }

            rigidbody.velocity += rigidbody.acceleration;
        });
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
