using Polyrhythm.Util;

namespace Polyrhythm.Data;

public class Model : IDisposable
{
    private readonly ModelImporter modelImporter;
    
    public Node<ModelNode> RootNode { get; }

    // Lazily load everything
    public IReadOnlyList<Mesh<Vertex>> Meshes => meshes ??= lazyMeshes.ToList();
    public IReadOnlyList<ModelMaterial> Materials => materials ??= lazyMaterials.ToList();
    public IReadOnlyList<ModelAnimation> Animations => animations ??= lazyAnimations.ToList();
    
    // Internal collections
    private List<Mesh<Vertex>>? meshes;
    private List<ModelMaterial>? materials;
    private List<ModelAnimation>? animations;

    private readonly IEnumerable<Mesh<Vertex>> lazyMeshes;
    private readonly IEnumerable<ModelMaterial> lazyMaterials;
    private readonly IEnumerable<ModelAnimation> lazyAnimations;

    public Model(Stream stream)
    {
        modelImporter = new ModelImporter(stream);
        
        RootNode = modelImporter.LoadModel();

        // This doesn't actually "load" anything yet
        lazyMeshes = modelImporter.LoadMeshes();
        lazyMaterials = modelImporter.LoadMaterials();
        lazyAnimations = modelImporter.LoadAnimations();
    }

    public void Dispose()
    {
        modelImporter.Dispose();
    }
}