namespace Polyrhythm.Data;

public class ModelMesh
{
    public int MeshIndex { get; }
    public int MaterialIndex { get; }

    public ModelMesh(int meshIndex, int materialIndex)
    {
        MeshIndex = meshIndex;
        MaterialIndex = materialIndex;
    }
}
