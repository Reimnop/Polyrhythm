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
    private readonly Camera camera;
    private readonly VertexShaderFactory<InputVertex, StagingVertex> vertexShaderFactory;
    private readonly TriangleShaderFactory<Triangle<StagingVertex>, ShadedTriangle> triangleShaderFactory;
    
    public Pipeline(
        Model model,
        IModelTransformer? modelTransformer,
        Camera camera,
        VertexShaderFactory<InputVertex, StagingVertex> vertexShaderFactory, 
        TriangleShaderFactory<Triangle<StagingVertex>, ShadedTriangle> triangleShaderFactory)
    {
        this.model = model;
        this.modelTransformer = modelTransformer;
        this.camera = camera;
        this.vertexShaderFactory = vertexShaderFactory;
        this.triangleShaderFactory = triangleShaderFactory;
    }

    public IEnumerable<ShadedTriangle> Render(double aspectRatio)
    {
        // Collect render data
        var renderDataList = CollectRenderDataRecursively(model.RootNode, Matrix4d.Identity);
        
        // Get camera data
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
        // TODO: Don't hardcode these values
        var ambientColor = new Vector3d(0.1, 0.1, 0.1);
        var lightDirection = new Vector3d(0.0, 0.0, -1.0);
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