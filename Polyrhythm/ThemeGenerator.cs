using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm;

public static class ThemeGenerator
{
    public static List<Vector3d> GenerateColorPalette(Model model, int shadingDepth)
    {
        var modelColors = GetModelColors(model);
        var palette = new List<Vector3d>();
        var colorDistance = 1.0 / shadingDepth * Math.Sqrt(3.0); // Will be infinite if shadingDepth is 0

        foreach (var color in modelColors)
        {
            foreach (var shade in GetShades(color, colorDistance))
            {
                if (GetMinDistance(shade, palette) < 32.0 / 255.0)
                    continue;
                palette.Add(shade);
            }
        }
        
        return palette;
    }

    private static IEnumerable<Vector3d> GetShades(Vector3d color, double colorDistance)
    {
        yield return color;
        
        if (colorDistance > Math.Sqrt(3.0))
            yield break;
        
        var shade = color;
        while (shade.Length > 0)
        {
            var shadeLength = shade.Length;
            var newShadeLength = Math.Max(shadeLength - colorDistance, 0.0);
            shade = shade.Normalized() * newShadeLength;
            yield return shade;
        }
    }

    private static List<Vector3d> GetModelColors(Model model)
    {
        var colors = new List<Vector3d>();
        
        foreach (var material in model.Materials)
        {
            var color = material.Albedo;
            if (GetMinDistance(color, colors) < 32.0 / 255.0)
                continue;
            colors.Add(color);
        }

        return colors;
    }
    
    private static double GetMinDistance(Vector3d color, List<Vector3d> colors)
    {
        if (colors.Count == 0)
            return double.MaxValue;
        
        return colors
            .Select(otherColor => CompareColors(color, otherColor))
            .Min();
    }

    private static double CompareColors(Vector3d color1, Vector3d color2)
    {
        return Vector3d.Distance(color1, color2);
    }
}