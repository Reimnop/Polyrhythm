using System;
using System.Collections.Generic;
using System.Text;

namespace Polyrhythm.Cli;

public class ArgumentHandler
{
    private readonly Dictionary<string, int> shortNameToIndex = new();
    private readonly Dictionary<string, int> longNameToIndex = new();
    private readonly List<ArgumentOption> options = new();
    
    public ArgumentHandler AddOption(string shortName, string longName, string title)
    {
        var index = options.Count;
        options.Add(new ArgumentOption(shortName, longName, title));
        shortNameToIndex.Add(shortName, index);
        longNameToIndex.Add(longName, index);
        
        return this;
    }
    
    public bool HasOptionShort(string shortName) 
        => shortNameToIndex.ContainsKey(shortName);
    
    public bool HasOptionLong(string longName) 
        => longNameToIndex.ContainsKey(longName);
    
    public ArgumentOption GetOptionShort(string shortName)
    {
        var index = shortNameToIndex[shortName];
        return options[index];
    }
    
    public ArgumentOption GetOptionLong(string longName)
    {
        var index = longNameToIndex[longName];
        return options[index];
    }

    public ArgumentParseResult ParseArguments(string[] args)
    {
        var result = new ArgumentParseResult(this);
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            
            if (arg.StartsWith("--")) // Use long name
            {
                var longName = arg[2..];
                if (!HasOptionLong(longName))
                    throw new ArgumentException($"Unknown option: {arg}");
                
                var nextIndex = i + 1;
                if (nextIndex >= args.Length) 
                    continue;
                
                var value = args[nextIndex];
                var option = GetOptionLong(longName);
                result.AddOptionValue(option.ShortName, option.LongName, value);
            }
            else if (arg.StartsWith("-")) // Use short name
            {
                var shortName = arg[1..];
                if (!HasOptionShort(shortName))
                    throw new ArgumentException($"Unknown option: {arg}");
                
                var nextIndex = i + 1;
                if (nextIndex >= args.Length) 
                    continue;
                
                var value = args[nextIndex];
                var option = GetOptionShort(shortName);
                result.AddOptionValue(option.ShortName, option.LongName, value);
            }
        }

        return result;
    }
    
    public void PrintHelp()
    {
        var builder = new StringBuilder();
        builder.AppendLine("Options:");
        foreach (var option in options)
        {
            var shortName = $"-{option.ShortName}";
            var longName = $"--{option.LongName}";
            var shortNamePadded = shortName.PadRight(4);
            var longNamePadded = longName.PadRight(16);
            builder.AppendLine($"  {shortNamePadded} {longNamePadded} : {option.Title}");
        }
        
        Console.WriteLine(builder.ToString());
    }
}