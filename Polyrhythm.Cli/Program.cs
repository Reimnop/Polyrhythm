using System;
using System.IO;
using OpenTK.Mathematics;
using PAPrefabToolkit;
using PAThemeToolkit;
using Polyrhythm.Cli;
using Polyrhythm.Conversion;
using Polyrhythm.Data;
using Polyrhythm.Util;

var argumentHandler = new ArgumentHandler()
    .AddOption("pn", "prefab-name", "Prefab name")
    .AddOption("tn", "theme-name", "Theme name")
    .AddOption("i", "input", "Input path")
    .AddOption("po", "prefab-output", "Prefab output path")
    .AddOption("to", "theme-output", "Theme output path")
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
var prefabName = parseResult.GetOptionValueShort("pn");
var themeName = parseResult.GetOptionValueShort("tn");
var input = parseResult.GetOptionValueShort("i");
var prefabOutput = parseResult.GetOptionValueShort("po");
var themeOutput = parseResult.GetOptionValueShort("to");
var width = Util.GetArgumentDoubleShort("w", parseResult);
var height = Util.GetArgumentDoubleShort("h", parseResult);
var framerate = Util.GetArgumentDoubleShort("r", parseResult);
var duration = Util.GetArgumentDoubleShort("d", parseResult);

using var stream = File.OpenRead(input);
using var model = new Model(stream);

// Generate theme
var theme = new Theme();
theme.Name = themeName;

for (int i = 0; i < 9; i++)
{
    var t = 1.0f - i / 8.0f;
    theme.Objects[i] = new Color(t, t, t);
}

var camera = new PerspectiveCamera
{
    Position = Vector3d.UnitZ * 1.5
};

// Convert model to prefab
var configuration = new Configuration(model, theme, camera, duration, 1.0 / framerate, new Vector2d(width, height));
var converter = new Converter(configuration);
var result = converter.CreatePrefab(initializeCallback: animationHandler =>
{
    if (model.Animations.Count == 0)
        return;
    animationHandler.Transition(model.Animations[0]);
});

var prefab = result.Prefab;
prefab.Name = prefabName;
prefab.Type = PrefabType.Characters;

// Export theme and prefab
theme.ExportToFile(themeOutput);
prefab.ExportToFile(prefabOutput);

Console.WriteLine($"Created prefab with {result.ObjectCount} objects and {result.FrameCount} frames.");