using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public struct RenderData
{
    public IEnumerable<InputVertex> Vertices { get; set; }
    public Matrix4d ModelMatrix { get; set; }
    
    public RenderData(IEnumerable<InputVertex> vertices, Matrix4d modelMatrix)
    {
        Vertices = vertices;
        ModelMatrix = modelMatrix;
    }
}