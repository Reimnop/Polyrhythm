using OpenTK.Mathematics;

namespace Polyrhythm.Data;

public struct Triangle<T> where T : unmanaged
{
    public T PointA { get; }
    public T PointB { get; }
    public T PointC { get; }
    
    public Triangle(T pointA, T pointB, T pointC)
    {
        PointA = pointA;
        PointB = pointB;
        PointC = pointC;
    }

    public T this[int index] =>
        index switch
        {
            0 => PointA,
            1 => PointB,
            2 => PointC,
            _ => throw new IndexOutOfRangeException()
        };

    public override string ToString()
    {
        return $"Triangle({PointA}, {PointB}, {PointC})";
    }
}