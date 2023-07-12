using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Rendering.Shader;

public class TriangleShaderSolidColor : IShader<Triangle<StagingVertex>, ShadedTriangle>
{
    public ShadedTriangle Process(Triangle<StagingVertex> input)
    {
        ProcessVertex(input[0], out var pointA, out var colorA);
        ProcessVertex(input[1], out var pointB, out var colorB);
        ProcessVertex(input[2], out var pointC, out var colorC);

        var triangle = new Triangle<Vector3d>(pointA, pointB, pointC);
        var color = (colorA + colorB + colorC) / 3.0;
        return new ShadedTriangle(triangle, color);
    }

    private void ProcessVertex(StagingVertex stagingVertex, out Vector3d position, out Vector3d color)
    {
        position = stagingVertex.Position.Xyz / stagingVertex.Position.W;
        color = stagingVertex.Color;
    }
    
    public static IShader<Triangle<StagingVertex>, ShadedTriangle> Factory(Vector3d ambientColor, Vector3d lightDirection)
    {
        return new TriangleShaderSolidColor();
    }
}