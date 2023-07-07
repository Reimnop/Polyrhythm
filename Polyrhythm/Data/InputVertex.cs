using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public struct InputVertex
{
    public Vector3d Position { get; set; }
    public Vector3d Normal { get; set; }
    public Vector3d Color { get; set; }
    public Vector3d Albedo { get; set; }
    
    public InputVertex(Vector3d position, Vector3d normal, Vector3d color, Vector3d albedo)
    {
        Position = position;
        Normal = normal;
        Color = color;
        Albedo = albedo;
    }
}