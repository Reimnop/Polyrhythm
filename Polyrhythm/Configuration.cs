using OpenTK.Mathematics;
using PAThemeToolkit;
using Polyrhythm.Data;
using Polyrhythm.Util;

namespace Polyrhythm;

public class Configuration
{
    public Model InputModel { get; }
    public Theme InputTheme { get; }
    public double Duration { get; }
    public double FrameDuration { get; }
    public Vector2d FrameSize { get; }
    
    public Configuration(Model inputModel, Theme inputTheme, double duration, double frameDuration, Vector2d frameSize)
    {
        InputModel = inputModel;
        InputTheme = inputTheme;
        Duration = duration;
        FrameDuration = frameDuration;
        FrameSize = frameSize;
    }
}