using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[GenerateAuthoringComponent]
public struct Collider : IComponentData
{
    public float2 size;
    public float2 origin;
    [Range(0, 1)]
    public float bounciness;

    [HideInInspector]
    public float2x4 vertices;
    [HideInInspector]
    public float2x4 normals;
}