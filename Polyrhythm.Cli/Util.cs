using System;
using OpenTK.Mathematics;

namespace Polyrhythm.Cli;

public static class Util
{
    public static int GetArgumentIntShort(string shortName, ArgumentParseResult parseResult)
    {
        var value = parseResult.GetOptionValueShort(shortName);
        int result;
        while (!int.TryParse(value, out result))
        {
            Console.WriteLine("Invalid input. Please enter a valid integer.");
            parseResult.PromptOptionValueShort(shortName);
            value = parseResult.GetOptionValueShort(shortName);
        }
        return result;
    }
    
    public static int GetArgumentIntLong(string longName, ArgumentParseResult parseResult)
    {
        var value = parseResult.GetOptionValueLong(longName);
        int result;
        while (!int.TryParse(value, out result))
        {
            Console.WriteLine("Invalid input. Please enter a valid integer.");
            parseResult.PromptOptionValueLong(longName);
            value = parseResult.GetOptionValueLong(longName);
        }
        return result;
    }
    
    public static double GetArgumentDoubleShort(string shortName, ArgumentParseResult parseResult)
    {
        var value = parseResult.GetOptionValueShort(shortName);
        double result;
        while (!double.TryParse(value, out result))
        {
            Console.WriteLine("Invalid input. Please enter a valid number.");
            parseResult.PromptOptionValueShort(shortName);
            value = parseResult.GetOptionValueShort(shortName);
        }
        return result;
    }
    
    public static double GetArgumentDoubleLong(string longName, ArgumentParseResult parseResult)
    {
        var value = parseResult.GetOptionValueLong(longName);
        double result;
        while (!double.TryParse(value, out result))
        {
            Console.WriteLine("Invalid input. Please enter a valid number.");
            parseResult.PromptOptionValueLong(longName);
            value = parseResult.GetOptionValueLong(longName);
        }
        return result;
    }

    public static string ColorToHex(Vector3d color)
    {
        var r = MathHelper.Clamp((int) (color.X * 255), 0, 255);
        var g = MathHelper.Clamp((int) (color.Y * 255), 0, 255);
        var b = MathHelper.Clamp((int) (color.Z * 255), 0, 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}