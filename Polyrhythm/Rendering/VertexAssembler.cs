using Polyrhythm.Data;

namespace Polyrhythm.Rendering;

/// <summary>
/// Assembles vertices into triangles
/// </summary>
public class VertexAssembler<T> where T : unmanaged
{
    private readonly IEnumerable<T> vertices;
    
    public VertexAssembler(IEnumerable<T> vertices)
    {
        this.vertices = vertices;
    }
    
    public IEnumerable<Triangle<T>> Assemble()
    {
        var queue = new Queue<T>();
        foreach (var vertex in vertices)
        {
            queue.Enqueue(vertex);
            
            if (queue.Count == 3)
            {
                var a = queue.Dequeue();
                var b = queue.Dequeue();
                var c = queue.Dequeue();
                yield return new Triangle<T>(a, b, c);
            }
        }

        if (queue.Count > 0)
            throw new InvalidOperationException($"{queue.Count} vertices are not part of a triangle!");
    }
}