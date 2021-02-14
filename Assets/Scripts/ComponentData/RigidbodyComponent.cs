using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Rigidbody : IComponentData
{
    public float2 velocity;
    public float2 acceleration;
}