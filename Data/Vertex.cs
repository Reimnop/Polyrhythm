using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public struct Vertex
{
    public Vector3d Position { get; set; }
    public Vector3d Normal { get; set; }
    public Vector3d Color { get; set; }
    
    public Vertex(Vector3d position, Vector3d normal, Vector3d color)
    {
        Position = position;
        Normal = normal;
        Color = color;
    }

    public Vertex(double x, double y, double z, double nx, double ny, double nz, double r, double g, double b)
        : this(new Vector3d(x, y, z), new Vector3d(nx, ny, nz), new Vector3d(r, g, b))
    {
    }
}