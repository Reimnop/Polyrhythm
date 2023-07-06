namespace Polyrhythm.Animation;

public struct Key<T>
{
    public double Time { get; }
    public T Value { get; }

    public Key(double time, T value)
    {
        Time = time;
        Value = value;
    }
}