using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Collider : IComponentData
{
    public float2 size;
    public float2 origin;
}