using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public enum ModelLightType
{
    Directional,
    Point,
    Spot
}

public class ModelLight
{
    public string Name { get; }
    public ModelLightType Type { get; }
    public Vector3d Color { get; }
    public double Intensity { get; }
    public double InnerConeAngle { get; }
    public double OuterConeAngle { get; }
    public double Range { get; }
    public double Falloff { get; }
    
    public ModelLight(string name, ModelLightType type, Vector3d color, double intensity, double innerConeAngle, double outerConeAngle, double range, double falloff)
    {
        Name = name;
        Type = type;
        Color = color;
        Intensity = intensity;
        InnerConeAngle = innerConeAngle;
        OuterConeAngle = outerConeAngle;
        Range = range;
        Falloff = falloff;
    }
}