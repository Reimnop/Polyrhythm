using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Rendering.Shader;

public class TriangleShader : IShader<Triangle<StagingVertex>, ShadedTriangle>
{
    private readonly Vector3d ambientColor;
    private readonly Vector3d lightDirection;

    public TriangleShader(Vector3d ambientColor, Vector3d lightDirection)
    {
        this.ambientColor = ambientColor;
        this.lightDirection = lightDirection;
    }
    
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
        var vertexNormal = stagingVertex.Normal;
        var vertexColor = stagingVertex.Color;
        var light = Vector3d.Dot(vertexNormal, -lightDirection);
        position = stagingVertex.Position.Xyz / stagingVertex.Position.W;
        color = vertexColor * light + vertexColor * ambientColor;
    }
    
    public static TriangleShader Factory(Vector3d ambientColor, Vector3d lightDirection)
    {
        return new TriangleShader(ambientColor, lightDirection);
    }
}