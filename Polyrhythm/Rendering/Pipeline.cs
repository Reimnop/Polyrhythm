using OpenTK.Mathematics;
using Polyrhythm.Data;
using Polyrhythm.Rendering.Shader;
using Polyrhythm.Util;

namespace Polyrhythm.Rendering;

public delegate IShader<TInput, TOutput> VertexShaderFactory<TInput, TOutput>(Matrix4d model, Matrix4d view, Matrix4d projection)
    where TInput : unmanaged
    where TOutput : unmanaged;

public delegate IShader<TInput, TOutput> TriangleShaderFactory<TInput, TOutput>(Vector3d ambientColor, Vector3d lightDirection)
    where TInput : unmanaged
    where TOutput : unmanaged;

public class Pipeline
{
    private readonly Model model;
    private readonly IModelTransformer? modelTransformer;
    private readonly VertexShaderFactory<InputVertex, StagingVertex> vertexShaderFactory;
    private readonly TriangleShaderFactory<Triangle<StagingVertex>, ShadedTriangle> triangleShaderFactory;
    
    public Pipeline(
        Model model,
        IModelTransformer? modelTransformer,
        VertexShaderFactory<InputVertex, StagingVertex> vertexShaderFactory, 
        TriangleShaderFactory<Triangle<StagingVertex>, ShadedTriangle> triangleShaderFactory)
    {
        this.model = model;
        this.modelTransformer = modelTransformer;
        this.vertexShaderFactory = vertexShaderFactory;
        this.triangleShaderFactory = triangleShaderFactory;
    }

    public IEnumerable<ShadedTriangle> Render(double aspectRatio)
    {
        // Collect render data
        var renderDataList = CollectRenderDataRecursively(model.RootNode, Matrix4d.Identity);
        
        // Get camera data
        if (model.Cameras.Count == 0)
            throw new InvalidOperationException("No cameras found!");
        
        var modelCamera = model.Cameras[0];

        // Get node associated with camera
        if (!model.RootNode.TryFindNode(modelCamera.Name, out var cameraNode))
            throw new InvalidOperationException($"Camera node '{modelCamera.Name}' not found!");

        var transform = GetNodeTransform(cameraNode.Value);
        var position = transform.ExtractTranslation();
        var rotation = transform.ExtractRotation();

        var camera = new PerspectiveCamera
        {
            Position = position,
            Rotation = modelCamera.Rotation * rotation,
            DepthNear = modelCamera.NearClippingPlane,
            DepthFar = modelCamera.FarClippingPlane,
            Fov = modelCamera.HorizontalFieldOfView / aspectRatio // Convert to vertical FOV
        };
        var cameraData = camera.GetCameraData(aspectRatio);
        
        // Process render data
        var stagingVertices = ProcessRenderData(renderDataList, cameraData);
        
        // Assemble vertices into triangles
        var assembler = new VertexAssembler<StagingVertex>(stagingVertices);
        var triangles = assembler.Assemble();
        
        // Process triangles
        var shadedTriangles = ProcessTriangles(triangles);
        
        // Return shaded triangles
        return shadedTriangles;
    }

    // Process triangles using the triangle shader
    private IEnumerable<ShadedTriangle> ProcessTriangles(IEnumerable<Triangle<StagingVertex>> triangles)
    {
        var ambientColor = Vector3d.One * 0.1;
        
        var directionalLight = model.Lights.FirstOrDefault(x => x.Type == ModelLightType.Directional);
        if (directionalLight == null)
            throw new NotImplementedException();
        if (!model.RootNode.TryFindNode(directionalLight.Name, out var lightNode))
            throw new InvalidOperationException($"Light node '{directionalLight.Name}' not found!");
        
        var lightTransform = GetNodeTransform(lightNode.Value);
        var lightRotation = directionalLight.Rotation * lightTransform.ExtractRotation();
        var lightDirection = Vector3d.Normalize(lightRotation * Vector3d.UnitZ);
        var triangleShader = triangleShaderFactory(ambientColor, lightDirection);
        foreach (var triangle in triangles)
        {
            yield return triangleShader.Process(triangle);
        }
    }

    // Process render data using the vertex shader
    private IEnumerable<StagingVertex> ProcessRenderData(IEnumerable<RenderData> renderDataList, CameraData cameraData)
    {
        foreach (var renderData in renderDataList)
        {
            var vertexShader = vertexShaderFactory(renderData.ModelMatrix, cameraData.View, cameraData.Projection);
            foreach (var vertex in renderData.Vertices)
            {
                yield return vertexShader.Process(vertex);
            }
        }
    }

    private IEnumerable<RenderData> CollectRenderDataRecursively(Node<ModelNode> node, Matrix4d parentTransform)
    {
        var modelNode = node.Value;
        var modelMatrix = GetNodeTransform(modelNode) * parentTransform;
        
        // Create and yield render data from meshes
        foreach (var modelMesh in modelNode.Meshes)
        {
            var mesh = model.Meshes[modelMesh.MeshIndex];
            var material = model.Materials[modelMesh.MaterialIndex];
            var vertices = mesh.Select(x => new InputVertex(x.Position, x.Normal, x.Color, material.Albedo));
            var renderData = new RenderData(vertices, modelMatrix);
            yield return renderData;
        }

        // Recursively collect render data from children
        foreach (var child in node.Children)
        {
            foreach (var renderData in CollectRenderDataRecursively(child, modelMatrix))
            {
                yield return renderData;
            }
        }
    }

    private Matrix4d GetNodeTransform(ModelNode node)
    {
        return modelTransformer?.GetNodeTransform(node) ?? node.Transform;
    }
}