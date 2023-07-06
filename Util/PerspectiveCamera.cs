using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Util;

public class PerspectiveCamera : Camera
{
    public double Fov { get; set; } = MathHelper.DegreesToRadians(60.0);

    public override double DepthNear { get; set; } = 0.1;
    public override double DepthFar { get; set; } = 10.0;

    public override CameraData GetCameraData(double aspectRatio)
    {
        var view = Matrix4d.Invert(Matrix4d.CreateFromQuaternion(Rotation) * Matrix4d.CreateTranslation(Position));
        var projection = Matrix4d.CreatePerspectiveFieldOfView(Fov, aspectRatio, DepthNear, DepthFar);
        
        return new CameraData(Position, Rotation, view, projection);
    }
}