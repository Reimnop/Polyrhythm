using Polyrhythm.Util;

namespace Polyrhythm.Data;

public class Model : IDisposable
{
    private readonly ModelImporter modelImporter;
    
    public Node<ModelNode> RootNode { get; }

    // Lazily load everything
    public IReadOnlyList<ModelCamera> Cameras => cameras ??= lazyCameras.ToList();
    public IReadOnlyList<ModelLight> Lights => lights ??= lazyLights.ToList();
    public IReadOnlyList<Mesh<Vertex>> Meshes => meshes ??= lazyMeshes.ToList();
    public IReadOnlyList<ModelMaterial> Materials => materials ??= lazyMaterials.ToList();
    public IReadOnlyList<ModelAnimation> Animations => animations ??= lazyAnimations.ToList();
    
    // Internal collections
    private List<ModelCamera>? cameras;
    private List<ModelLight>? lights;
    private List<Mesh<Vertex>>? meshes;
    private List<ModelMaterial>? materials;
    private List<ModelAnimation>? animations;

    private readonly IEnumerable<ModelCamera> lazyCameras;
    private readonly IEnumerable<ModelLight> lazyLights;
    private readonly IEnumerable<Mesh<Vertex>> lazyMeshes;
    private readonly IEnumerable<ModelMaterial> lazyMaterials;
    private readonly IEnumerable<ModelAnimation> lazyAnimations;

    public Model(Stream stream)
    {
        modelImporter = new ModelImporter(stream);
        
        RootNode = modelImporter.LoadModel();

        // This doesn't actually "load" anything yet
        lazyCameras = modelImporter.LoadCameras();
        lazyLights = modelImporter.LoadLights();
        lazyMeshes = modelImporter.LoadMeshes();
        lazyMaterials = modelImporter.LoadMaterials();
        lazyAnimations = modelImporter.LoadAnimations();
    }

    public void Dispose()
    {
        modelImporter.Dispose();
    }
}