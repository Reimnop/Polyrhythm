using OpenTK.Mathematics;
using PAPrefabToolkit;
using Polyrhythm.Cli;
using Polyrhythm;
using Polyrhythm.Data;

var argumentHandler = new ArgumentHandler()
    .AddOption("n", "name", "Output prefab name")
    .AddOption("o", "output", "Prefab output path")
    .AddOption("m", "model", "Input model path")
    .AddOption("s", "depth", "Shading depth (Use 0 for no shading)")
    .AddOption("w", "width", "Viewport width")
    .AddOption("h", "height", "Viewport height")
    .AddOption("r", "framerate", "Framerate")
    .AddOption("d", "duration", "Duration");
    
if (args.Length == 1 && (args[0] == "-h" || args[0] == "--help"))
{
    argumentHandler.PrintHelp();
    Console.WriteLine();
    Console.WriteLine("To show this help message, use the -h or --help option.");
    return;
}

var parseResult = argumentHandler.ParseArguments(args);
var prefabName = parseResult.GetOptionValueShort("n");
var prefabOutput = parseResult.GetOptionValueShort("o");
var input = parseResult.GetOptionValueShort("m");
var shadingDepth = Util.GetArgumentIntShort("s", parseResult);
var width = Util.GetArgumentDoubleShort("w", parseResult);
var height = Util.GetArgumentDoubleShort("h", parseResult);
var framerate = Util.GetArgumentDoubleShort("r", parseResult);
var duration = Util.GetArgumentDoubleShort("d", parseResult);

using var stream = File.OpenRead(input);
using var model = new Model(stream);

// Convert model to prefab
var configuration = new Configuration(model, shadingDepth, duration, 1.0 / framerate, new Vector2d(width, height));
var converter = new Converter(configuration);
var result = converter.StartRender(initializeCallback: animationHandler =>
{
    if (model.Animations.Count == 0)
        return;
    animationHandler.Transition(model.Animations[0]);
});

var prefab = result.Prefab;
prefab.Name = prefabName;
prefab.Type = PrefabType.Characters;

// Export theme and prefab
prefab.ExportToFile(prefabOutput);

// Print statistics
Console.WriteLine($"Rendered {result.FrameCount} frames.");
Console.WriteLine($"Exported prefab to '{prefabOutput}'. ({result.ObjectCount} objects, {result.Palette.Count} colors)");
Console.WriteLine();
Console.WriteLine("Paste the following values into a theme in Project Arrhythmia:");
for (int i = 0; i < result.Palette.Count; i++)
{
    var color = result.Palette[i];
    Console.WriteLine($"  {i + 1}: {Util.ColorToHex(color)}");
}

