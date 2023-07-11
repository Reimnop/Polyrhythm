using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public class ModelCamera
{
    public string Name { get; }
    public double HorizontalFieldOfView { get; }
    public double NearClippingPlane { get; }
    public double FarClippingPlane { get; }
    public Quaterniond Rotation { get; }

    public ModelCamera(string name, double horizontalFieldOfView, double nearClippingPlane, double farClippingPlane, Quaterniond rotation)
    {
        Name = name;
        HorizontalFieldOfView = horizontalFieldOfView;
        NearClippingPlane = nearClippingPlane;
        FarClippingPlane = farClippingPlane;
        Rotation = rotation;
    }
}