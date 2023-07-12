using OpenTK.Mathematics;
using PAPrefabToolkit;

namespace Polyrhythm.Data;

public class PrefabCreationResult
{
    public Prefab Prefab { get; }
    public List<Vector3d> Palette { get; }
    public int ObjectCount { get; }
    public int FrameCount { get; }
    
    public PrefabCreationResult(Prefab prefab, List<Vector3d> palette, int objectCount, int frameCount)
    {
        Prefab = prefab;
        Palette = palette;
        ObjectCount = objectCount;
        FrameCount = frameCount;
    }
}