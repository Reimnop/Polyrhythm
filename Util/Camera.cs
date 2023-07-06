using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Util;

public abstract class Camera
{
    public Vector3d Position { get; set; } = Vector3d.Zero;
    public Quaterniond Rotation { get; set; } = Quaterniond.Identity;
    
    public abstract double DepthNear { get; set; }
    public abstract double DepthFar { get; set; }

    public abstract CameraData GetCameraData(double aspectRatio);
}