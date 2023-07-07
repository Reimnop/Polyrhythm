using OpenTK.Mathematics;
using PAPrefabToolkit;
using PAThemeToolkit;
using Polyrhythm.Conversion;
using Polyrhythm.Data;
using Polyrhythm.Util;

using var stream = File.OpenRead("input.fbx");
using var model = new Model(stream);

var theme = new Theme();
for (int i = 0; i < 9; i++)
{
    var t = 1.0f - i / 8.0f;
    theme.Objects[i] = new Color(t, t, t);
}

var camera = new PerspectiveCamera
{
    Position = Vector3d.UnitZ * 1.5
};

var configuration = new Configuration(model, theme, camera, 4.0, 1.0 / 24.0, Vector2d.One * 10.0);
var converter = new Converter(configuration);
var result = converter.CreatePrefab(initializeCallback: animationHandler =>
{
    animationHandler.Transition(model.Animations[0]);
});
var prefab = result.Prefab;
prefab.ExportToFile("output.lsp");
theme.ExportToFile("output.lst");

Console.WriteLine($"Created prefab with {result.ObjectCount} objects and {result.FrameCount} frames.");