using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public struct ShadedTriangle
{
    public Triangle<Vector3d> Triangle { get; set; }
    public Vector3d Color { get; set; }

    public ShadedTriangle(Triangle<Vector3d> triangle, Vector3d color)
    {
        Triangle = triangle;
        Color = color;
    }
}