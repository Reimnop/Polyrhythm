using System.Runtime.CompilerServices;

namespace Polyrhythm.Animation;

public class NoKeyException : Exception
{
    public NoKeyException() : base("Cannot interpolate in an empty sequence!")
    {
    }
}

public delegate T Interpolator<T>(T first, T second, double factor);

/// <summary>
/// Provides functionality to interpolate on a list of keyframes
/// </summary>
public static class Sequence
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Interpolate<T>(double time, IReadOnlyList<Key<T>> keys, Interpolator<T> interpolator)
    {
        if (keys.Count == 0)
        {
            throw new NoKeyException();
        }

        if (keys.Count == 1)
        {
            return keys[0].Value;
        }

        if (time < keys[0].Time)
        {
            return keys[0].Value;
        }
        
        if (time >= keys[^1].Time)
        {
            return keys[^1].Value;
        }

        int index = Search(time, keys);
        Key<T> first = keys[index];
        Key<T> second = keys[index + 1];

        double t = InverseLerp(first.Time, second.Time, time);
        return interpolator(first.Value, second.Value, t);
    }
    
    // Binary search for the keyframe pair that contains the given time
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Search<T>(double time, IReadOnlyList<Key<T>> keys)
    {
        int low = 0;
        int high = keys.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            double midTime = keys[mid].Time;

            if (time < midTime)
            {
                high = mid - 1;
            }
            else if (time > midTime)
            {
                low = mid + 1;
            }
            else
            {
                return mid;
            }
        }

        return low - 1;
    }
    
    // Like lerp, but inverted
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double InverseLerp(double a, double b, double factor)
    {
        return a != b ? (factor - a) / (b - a) : 0.0f; // Prevent division by zero
    }
}