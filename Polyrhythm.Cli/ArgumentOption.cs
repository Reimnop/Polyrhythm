namespace Polyrhythm.Cli;

public struct ArgumentOption
{
    public string ShortName { get; }
    public string LongName { get; }
    public string Title { get; }
    
    public ArgumentOption(string shortName, string longName, string title)
    {
        ShortName = shortName;
        LongName = longName;
        Title = title;
    }
}