using PAPrefabToolkit;

namespace Polyrhythm.Data;

public class PrefabCreationResult
{
    public Prefab Prefab { get; }
    public int ObjectCount { get; }
    public int FrameCount { get; }
    
    public PrefabCreationResult(Prefab prefab, int objectCount, int frameCount)
    {
        Prefab = prefab;
        ObjectCount = objectCount;
        FrameCount = frameCount;
    }
}