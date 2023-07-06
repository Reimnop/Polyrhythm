using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Rendering.Shader;

public class VertexShader : IShader<InputVertex, StagingVertex>
{
    private readonly Matrix4d model;
    private readonly Matrix4d modelViewProjection;
    
    public VertexShader(Matrix4d model, Matrix4d view, Matrix4d projection)
    {
        this.model = model;
        modelViewProjection = model * view * projection;
    }

    public StagingVertex Process(InputVertex input)
    {
        var position = new Vector4d(input.Position, 1.0) * modelViewProjection;
        var normal = Vector3d.Normalize(new Vector3d(new Vector4d(input.Normal, 0.0) * model));
        return new StagingVertex(position, normal, input.Color * input.Albedo);
    }
    
    public static VertexShader Factory(Matrix4d model, Matrix4d view, Matrix4d projection)
    {
        return new VertexShader(model, view, projection);
    }
}