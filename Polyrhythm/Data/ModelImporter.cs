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

    public IEnumerable<ModelCamera> LoadCameras()
    {
        return from sceneCamera in scene.Cameras
            let name = sceneCamera.Name
            let horizontalFieldOfView = sceneCamera.FieldOfview
            let nearPlane = sceneCamera.ClipPlaneNear
            let farPlane = sceneCamera.ClipPlaneFar
            let up = new Vector3d(sceneCamera.Up.X, sceneCamera.Up.Y, sceneCamera.Up.Z)
            let direction = new Vector3d(sceneCamera.Direction.X, sceneCamera.Direction.Y, sceneCamera.Direction.Z)
            let right = Vector3d.Cross(up, direction)
            let rotationMatrix = new Matrix3d(right, up, direction)
            let rotation = Quaterniond.FromMatrix(rotationMatrix)
            select new ModelCamera(name, horizontalFieldOfView, nearPlane, farPlane, rotation);
    }

    public IEnumerable<ModelLight> LoadLights()
    {
        return from sceneLight in scene.Lights
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
            let up = new Vector3d(sceneLight.Up.X, sceneLight.Up.Y, sceneLight.Up.Z)
            let direction = new Vector3d(sceneLight.Direction.X, sceneLight.Direction.Y, sceneLight.Direction.Z)
            let right = Vector3d.Cross(up, direction)
            let rotationMatrix = new Matrix3d(right, up, direction)
            let rotation = Quaterniond.FromMatrix(rotationMatrix)
            select new ModelLight(name, type, color, intensity, innerConeAngle, outerConeAngle, range, falloff, rotation);
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
        return from sceneAnimation in scene.Animations
            let name = sceneAnimation.Name
            let durationInTicks = sceneAnimation.DurationInTicks
            let ticksPerSecond = sceneAnimation.TicksPerSecond
            let nodeAnimationChannels = LoadModelNodeAnimationChannel(sceneAnimation.NodeAnimationChannels)
            select new ModelAnimation(name, durationInTicks, ticksPerSecond, nodeAnimationChannels);
    }

    private static IEnumerable<ModelNodeAnimationChannel> LoadModelNodeAnimationChannel(IEnumerable<NodeAnimationChannel> nodeAnimationChannels)
    {
        return from nodeAnimationChannel in nodeAnimationChannels 
            let name = nodeAnimationChannel.NodeName 
            let positionKeys = from x in nodeAnimationChannel.PositionKeys 
                select new Key<Vector3d>(x.Time, new Vector3d(x.Value.X, x.Value.Y, x.Value.Z)) 
            let scaleKeys = from x in nodeAnimationChannel.ScalingKeys 
                select new Key<Vector3d>(x.Time, new Vector3d(x.Value.X, x.Value.Y, x.Value.Z)) 
            let rotationKeys = from x in nodeAnimationChannel.RotationKeys 
                select new Key<Quaterniond>(x.Time, new Quaterniond(x.Value.X, x.Value.Y, x.Value.Z, x.Value.W)) 
            select new ModelNodeAnimationChannel(name, positionKeys, scaleKeys, rotationKeys);
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