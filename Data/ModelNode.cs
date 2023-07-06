using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public class ModelNode
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
