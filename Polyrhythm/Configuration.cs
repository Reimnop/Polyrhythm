using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm;

public class Configuration
{
    public Model InputModel { get; }
    public int ShadingDepth { get; }
    public double Duration { get; }
    public double FrameDuration { get; }
    public Vector2d FrameSize { get; }
    
    public Configuration(Model inputModel, int shadingDepth, double duration, double frameDuration, Vector2d frameSize)
    {
        InputModel = inputModel;
        ShadingDepth = shadingDepth;
        Duration = duration;
        FrameDuration = frameDuration;
        FrameSize = frameSize;
    }
}