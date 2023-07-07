using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public struct PrefabTriangle
{
    public Vector2d Position { get; set; }
    public Vector2d Scale { get; set; }
    public double Rotation { get; set; }
    public double RenderDepth { get; set; }
    public int ThemeColor { get; set; }
    
    public PrefabTriangle(Vector2d position, Vector2d scale, double rotation, double renderDepth, int themeColor)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
        RenderDepth = renderDepth;
        ThemeColor = themeColor;
    }
}