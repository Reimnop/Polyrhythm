using OpenTK.Mathematics;

namespace Polyrhythm.Data;

/// <summary>
/// Intermediate vertex format between the vertex shader and the triangle shader
/// </summary>
public struct StagingVertex
{
    public Vector4d Position { get; set; }
    public Vector3d Normal { get; set; }
    public Vector3d Color { get; set; }
    
    public StagingVertex(Vector4d position, Vector3d normal, Vector3d color)
    {
        Position = position;
        Normal = normal;
        Color = color;
    }
}