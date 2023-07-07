using Polyrhythm.Data;

namespace Polyrhythm.Data;

public class PrefabFrame
{
    public IReadOnlyList<ShadedTriangle> Triangles { get; }
    
    public PrefabFrame(IReadOnlyList<ShadedTriangle> triangles)
    {
        Triangles = triangles;
    }
}