using System;
using System.Collections.Generic;

namespace Polyrhythm.Cli;

public class ArgumentParseResult
{
    private readonly ArgumentHandler handler;
    private readonly Dictionary<string, int> shortNameToIndex;
    private readonly Dictionary<string, int> longNameToIndex;
    private readonly List<string> arguments;
    
    public ArgumentParseResult(ArgumentHandler handler)
    {
        this.handler = handler;
        shortNameToIndex = new();
        longNameToIndex = new();
        arguments = new();
    }
    
    public ArgumentParseResult AddOptionValue(string shortName, string longName, string value)
    {
        var index = arguments.Count;
        arguments.Add(value);
        shortNameToIndex.Add(shortName, index);
        longNameToIndex.Add(longName, index);
        
        return this;
    }
    
    public ArgumentParseResult SetOptionValueShort(string shortName, string value)
    {
        var index = shortNameToIndex[shortName];
        arguments[index] = value;
        
        return this;
    }
    
    public ArgumentParseResult SetOptionValueLong(string longName, string value)
    {
        var index = longNameToIndex[longName];
        arguments[index] = value;
        
        return this;
    }
    
    public bool HasOptionValueShort(string shortName) 
        => shortNameToIndex.ContainsKey(shortName);
    
    public bool HasOptionValueLong(string longName)
        => longNameToIndex.ContainsKey(longName);
    
    public void PromptOptionValueShort(string shortName)
    {
        var option = handler.GetOptionShort(shortName);
        Console.Write($"{option.Title} : ");
        var value = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(value))
            return;
        
        if (HasOptionValueShort(shortName))
        {
            SetOptionValueShort(shortName, value);
            return;
        }
        
        AddOptionValue(shortName, option.LongName, value);
    }
    
    public void PromptOptionValueLong(string longName)
    {
        var option = handler.GetOptionLong(longName);
        Console.Write($"{option.Title} : ");
        var value = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(value))
            return;
        
        if (HasOptionValueLong(longName))
        {
            SetOptionValueLong(longName, value);
            return;
        }
        
        AddOptionValue(option.ShortName, longName, value);
    }

    public string GetOptionValueShort(string shortName)
    {
        while (!HasOptionValueShort(shortName))
            PromptOptionValueShort(shortName);

        var index = shortNameToIndex[shortName];
        return arguments[index];
    }
    
    public string GetOptionValueLong(string longName)
    {
        while (!HasOptionValueLong(longName))
            PromptOptionValueLong(longName);

        var index = longNameToIndex[longName];
        return arguments[index];
    }
}