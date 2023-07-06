using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Util;

public class OrthographicCamera : Camera
{
    public double Size { get; set; } = 10.0f;

    public override double DepthNear { get; set; } = -1.0;
    public override double DepthFar { get; set; } = 1.0;

    public override CameraData GetCameraData(double aspectRatio)
    {
        var view = Matrix4d.Invert(Matrix4d.CreateFromQuaternion(Rotation) * Matrix4d.CreateTranslation(Position));
        var projection = Matrix4d.CreateOrthographic(Size * aspectRatio, Size, DepthNear, DepthFar);
        
        return new CameraData(Position, Rotation, view, projection);
    }
}