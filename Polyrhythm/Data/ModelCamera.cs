namespace Polyrhythm.Data;

public class ModelCamera
{
    public string Name { get; }
    public double HorizontalFieldOfView { get; }
    
    public ModelCamera(string name, double horizontalFieldOfView)
    {
        Name = name;
        HorizontalFieldOfView = horizontalFieldOfView;
    }
}