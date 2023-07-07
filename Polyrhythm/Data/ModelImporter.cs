using Assimp;
using OpenTK.Mathematics;
using Polyrhythm.Animation;
using Polyrhythm.Util;

namespace Polyrhythm.Data;

public class ModelImporter : IDisposable
{
    private readonly AssimpContext context;
    private readonly Scene scene;

    public ModelImporter(Stream stream)
    {
        context = new AssimpContext();
        scene = context.ImportFileFromStream(stream, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.FlipUVs);
    }

    public IEnumerable<ModelMaterial> LoadMaterials()
    {
        return scene.Materials.Select(material =>
        {
            var albedo = new Vector3d(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B);
            var metallic = material.Reflectivity;
            var roughness = 1.0 - material.Shininess;
            return new ModelMaterial(material.Name, albedo, metallic, roughness);
        });
    }

    public IEnumerable<Mesh<Vertex>> LoadMeshes()
    {
        return scene.Meshes.Select(mesh => new Mesh<Vertex>(mesh.Name, EnumerateVertices(mesh), mesh.GetIndices()));
    }

    private static IEnumerable<Vertex> EnumerateVertices(Mesh mesh)
    {
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            var position = mesh.Vertices[i];
            var normal = mesh.Normals[i];
            var color = mesh.VertexColorChannelCount == 0
                ? new Color4D(1.0f)
                : mesh.VertexColorChannels[0][i];
                
            yield return new Vertex(
                position.X, position.Y, position.Z, 
                normal.X, normal.Y, normal.Z,
                color.R, color.G, color.B);
        }
    }

    public IEnumerable<ModelAnimation> LoadAnimations()
    {
        return scene.Animations.Select(animation => 
            new ModelAnimation(
                animation.Name,
                animation.DurationInTicks,
                animation.TicksPerSecond,
                animation.NodeAnimationChannels.Select(channel =>
                    new ModelNodeAnimationChannel(
                        channel.NodeName,
                        channel.PositionKeys.Select(x => new Key<Vector3d>(x.Time, new Vector3d(x.Value.X, x.Value.Y, x.Value.Z))), 
                        channel.ScalingKeys.Select(x => new Key<Vector3d>(x.Time, new Vector3d(x.Value.X, x.Value.Y, x.Value.Z))),
                        channel.RotationKeys.Select(x => new Key<Quaterniond>(x.Time, new Quaterniond(x.Value.X, x.Value.Y, x.Value.Z, x.Value.W)))
                    )
                )
            )
        );
    }

    public Node<ModelNode> LoadModel()
    {
        var rootNode = scene.RootNode;
        var modelTreeBuilder = GetModelTreeBuilder(rootNode);
        return modelTreeBuilder.Build();
    }

    private TreeBuilder<ModelNode> GetModelTreeBuilder(Node node)
    {
        var modelMeshes = new List<ModelMesh>();
        if (node.HasMeshes)
        {
            node.MeshIndices.ForEach(meshIndex => modelMeshes.Add(GetMesh(meshIndex)));
        }

        var aiTransform = node.Transform;
        var transform = new Matrix4d(
            aiTransform.A1, aiTransform.B1, aiTransform.C1, aiTransform.D1,
            aiTransform.A2, aiTransform.B2, aiTransform.C2, aiTransform.D2,
            aiTransform.A3, aiTransform.B3, aiTransform.C3, aiTransform.D3,
            aiTransform.A4, aiTransform.B4, aiTransform.C4, aiTransform.D4);

        var modelNode = new ModelNode(node.Name, transform, modelMeshes);

        var treeBuilder = new TreeBuilder<ModelNode>(modelNode);
        foreach (var child in node.Children)
        {
            treeBuilder.PushChild(GetModelTreeBuilder(child));
        }

        return treeBuilder;
    }

    private ModelMesh GetMesh(int meshIndex)
    {
        var mesh = scene.Meshes[meshIndex];
        return new ModelMesh(meshIndex, mesh.MaterialIndex);
    }

    public void Dispose()
    {
        context.Dispose();
    }
}