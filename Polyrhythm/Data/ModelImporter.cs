using Assimp;
using OpenTK.Mathematics;
using Polyrhythm.Animation;
using Polyrhythm.Util;

namespace Polyrhythm.Data;

public class ModelImporter : IDisposable
{
    public ModelCamera? Camera { get; }
    public IReadOnlyList<ModelLight> Lights { get; }

    private readonly AssimpContext context;
    private readonly Scene scene;

    public ModelImporter(Stream stream)
    {
        context = new AssimpContext();
        scene = context.ImportFileFromStream(stream, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.FlipUVs);
        
        // Get camera data
        if (scene.CameraCount > 0) // Check if there is a camera
        {
            var camera = scene.Cameras[0];
            var name = camera.Name;
            var horizontalFieldOfView = camera.FieldOfview * 2.0;
            Camera = new ModelCamera(name, horizontalFieldOfView);
        }
        
        // Get light data
        // I'm not sure if I like LINQ here
        var lights = (from sceneLight in scene.Lights
                let name = sceneLight.Name
                let type = sceneLight.LightType switch
                {
                    LightSourceType.Directional => ModelLightType.Directional,
                    LightSourceType.Point => ModelLightType.Point,
                    LightSourceType.Spot => ModelLightType.Spot,
                    _ => throw new Exception($"Unknown light type '{sceneLight.LightType}'!")
                }
                let color = new Vector3d(sceneLight.ColorDiffuse.R, sceneLight.ColorDiffuse.G, sceneLight.ColorDiffuse.B)
                let intensity = sceneLight.AttenuationConstant
                let innerConeAngle = sceneLight.AngleInnerCone
                let outerConeAngle = sceneLight.AngleOuterCone
                let range = sceneLight.AttenuationLinear
                let falloff = sceneLight.AttenuationQuadratic
                select new ModelLight(name, type, color, intensity, innerConeAngle, outerConeAngle, range, falloff))
            .ToList();

        Lights = lights;
    }

    public IEnumerable<ModelMaterial> LoadMaterials()
    {
        return from material in scene.Materials
            let albedo = new Vector3d(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B)
            let metallic = material.Reflectivity
            let roughness = 1.0 - material.Shininess
            select new ModelMaterial(material.Name, albedo, metallic, roughness);
    }

    public IEnumerable<Mesh<Vertex>> LoadMeshes()
    {
        return from mesh in scene.Meshes 
            select new Mesh<Vertex>(mesh.Name, EnumerateVertices(mesh), mesh.GetIndices());
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
        return from animation in scene.Animations
            select new ModelAnimation(
                animation.Name,
                animation.DurationInTicks,
                animation.TicksPerSecond,
                from channel in animation.NodeAnimationChannels
                select new ModelNodeAnimationChannel(
                    channel.NodeName,
                    from x in channel.PositionKeys
                    select new Key<Vector3d>(x.Time, new Vector3d(x.Value.X, x.Value.Y, x.Value.Z)),
                    from x in channel.ScalingKeys
                    select new Key<Vector3d>(x.Time, new Vector3d(x.Value.X, x.Value.Y, x.Value.Z)),
                    from x in channel.RotationKeys
                    select new Key<Quaterniond>(x.Time, new Quaterniond(x.Value.X, x.Value.Y, x.Value.Z, x.Value.W))
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