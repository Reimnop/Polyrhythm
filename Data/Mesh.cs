using System.Collections;

namespace Polyrhythm.Data;

public class Mesh<T> : IEnumerable<T> where T : unmanaged
{
    public string Name { get; }
    public IReadOnlyList<T> Vertices => vertices;
    public IReadOnlyList<int> Indices => indices;
    
    private readonly List<T> vertices;
    private readonly List<int> indices;
    
    public Mesh(string name, IEnumerable<T> vertices, IEnumerable<int> indices)
    {
        Name = name;
        this.vertices = vertices.ToList();
        this.indices = indices.ToList();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return indices.Select(index => vertices[index]).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}