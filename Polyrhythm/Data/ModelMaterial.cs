using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public class ModelMaterial
{
    public string Name { get; }
    public Vector3d Albedo { get; set; }
    public double Metallic { get; set; } 
    public double Roughness { get; set; }

    public ModelMaterial(string name, Vector3d albedo, double metallic, double roughness)
    {
        Name = name;
        Albedo = albedo;
        Metallic = metallic;
        Roughness = roughness;
    }
}
