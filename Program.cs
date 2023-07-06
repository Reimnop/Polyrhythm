using OpenTK.Mathematics;
using PAPrefabToolkit;
using PAThemeToolkit;
using Polyrhythm.Conversion;
using Polyrhythm.Data;

using var stream = File.OpenRead("input.fbx");
using var model = new Model(stream);

var theme = new Theme();
for (int i = 0; i < 9; i++)
{
    var t = 1.0f - i / 8.0f;
    theme.Objects[i] = new Color(t, t, t);
}

var configuration = new Configuration(model, theme, 10.0, 0.25, Vector2d.One * 5.0);
var converter = new Converter(configuration);
var prefab = converter.CreatePrefab(initializeCallback: animationHandler =>
{
    animationHandler.Transition(model.Animations[0]);
});
prefab.ExportToFile("output.lsp", PrefabBuildFlags.AbsoluteRotation);
theme.ExportToFile("output.lst");