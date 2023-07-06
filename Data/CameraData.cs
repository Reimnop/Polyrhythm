using OpenTK.Mathematics;

namespace Polyrhythm.Data;

/// <summary>
/// Container for information about the camera
/// </summary>
public struct CameraData
{
    /// <summary>
    /// Position of the camera in world space
    /// </summary>
    public Vector3d Position { get; }
    
    /// <summary>
    /// Rotation of the camera in world space
    /// </summary>
    public Quaterniond Rotation { get; }

    /// <summary>
    /// Matrix for transforming from world space to view space
    /// </summary>
    public Matrix4d View { get; }
    
    /// <summary>
    /// Matrix for transforming from view space to clip space
    /// </summary>
    public Matrix4d Projection { get; }

    public CameraData(Vector3d position, Quaterniond rotation, Matrix4d view, Matrix4d projection)
    {
        Position = position;
        Rotation = rotation;
        View = view;
        Projection = projection;
    }
}