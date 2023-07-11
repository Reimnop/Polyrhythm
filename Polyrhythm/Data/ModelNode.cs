using OpenTK.Mathematics;
using Polyrhythm.Util;

namespace Polyrhythm.Data;

public class ModelNode : INamed
{
    public string Name { get; }
    public Matrix4d Transform { get; }
    public IReadOnlyList<ModelMesh> Meshes => meshes;

    private readonly List<ModelMesh> meshes;

    public ModelNode(string name, Matrix4d transform, List<ModelMesh> meshes)
    {
        Name = name;
        Transform = transform;
        this.meshes = meshes;
    }
}
