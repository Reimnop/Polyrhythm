using OpenTK.Mathematics;
using PAPrefabToolkit;
using Polyrhythm.Data;
using Polyrhythm.Rendering;
using Polyrhythm.Rendering.Shader;
using Polyrhythm.Util;
using Vector2 = System.Numerics.Vector2;

namespace Polyrhythm;

public delegate void InitializeCallback(AnimationHandler animationHandler);

public delegate void UpdateCallback(double time);

/// <summary>
/// Converts a model into a Project Arrhythmia prefab.
/// </summary>
public class Converter
{
    private readonly Configuration configuration;
    
    public Converter(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public PrefabCreationResult StartRender(InitializeCallback? initializeCallback = null, UpdateCallback? updateCallback = null)
    {
        var model = configuration.InputModel;
        
        // Generate color palette
        var palette = ThemeGenerator.GenerateColorPalette(model, configuration.ShadingDepth);

        // Initialize animation handler
        var animationHandler = new AnimationHandler(model);
        initializeCallback?.Invoke(animationHandler);
        
        // Initialize pipeline
        var pipeline = new Pipeline(
            model, 
            animationHandler, 
            VertexShader.Factory, 
            configuration.ShadingDepth == 0 
                ? TriangleShaderSolidColor.Factory 
                : TriangleShader.Factory);
        
        // Render all frames into a list
        var frames = new List<PrefabFrame>();
        for (var t = 0.0; t < configuration.Duration; t += configuration.FrameDuration)
        {
            updateCallback?.Invoke(t);
            animationHandler.Update(t);
            var triangles = pipeline.Render(configuration.FrameSize.X / configuration.FrameSize.Y);
            var frame = new PrefabFrame(triangles.ToList());
            frames.Add(frame);
        }

        var prefabObjectPool = new PrefabObjectPool(frames.Max(x => x.Triangles.Count * 2));
        int frameCount = 0;
        
        // Turn frames into prefab
        var prefab = new Prefab();
        var parent = prefab.CreateObject("Viewport");
        parent.PositionParenting = true;
        parent.ScaleParenting = true;
        parent.RotationParenting = true;
        parent.ObjectType = PrefabObjectType.Empty;
        parent.StartTime = 0.0f;
        parent.AutoKillType = PrefabObjectAutoKillType.Fixed;
        parent.AutoKillOffset = (float) configuration.Duration;
        parent.PositionKeyframes.Add(new PositionKeyframe());
        parent.ScaleKeyframes.Add(new ScaleKeyframe
        {
            Value = new Vector2((float) configuration.FrameSize.X, (float) configuration.FrameSize.Y)
        });
        parent.RotationKeyframes.Add(new RotationKeyframe());
        parent.ColorKeyframes.Add(new ColorKeyframe());

        for (int i = 0; i < frames.Count; i++)
        {
            var time = i * configuration.FrameDuration;
            updateCallback?.Invoke(time);
            prefabObjectPool.AddFrame((float) time, EnumeratePrefabTriangles(frames[i], palette));
            frameCount++;
        }
        
        var objects = prefabObjectPool
            .BuildPrefabObjects(prefab, (float) configuration.Duration)
            .ToList();
        
        foreach (var obj in objects)
            parent.AddChild(obj);

        return new PrefabCreationResult(prefab, palette, objects.Count, frameCount);
    }

    private static IEnumerable<PrefabTriangle> EnumeratePrefabTriangles(PrefabFrame frame, IReadOnlyList<Vector3d> colors)
    {
        foreach (var shadedTriangle in frame.Triangles)
        {
            var depth = (shadedTriangle.Triangle[0].Z + shadedTriangle.Triangle[1].Z + shadedTriangle.Triangle[2].Z) / 3.0;
            
            if (depth < 0.0 || depth > 1.0) // Outside of clipping range
                continue;
            if (GetWindingOrder(shadedTriangle.Triangle) < 0.0) // Backface culling
                continue;

            var triangle2d = new Triangle<Vector2d>(
                shadedTriangle.Triangle[0].Xy,
                shadedTriangle.Triangle[1].Xy,
                shadedTriangle.Triangle[2].Xy);

            GeometryHelper.GetRightTriangles(triangle2d, out var tri0, out var tri1);
            GeometryHelper.GetPositionScaleRotation(tri0, out var position0, out var scale0, out var rotation0);
            GeometryHelper.GetPositionScaleRotation(tri1, out var position1, out var scale1, out var rotation1);
            var color = GetThemeColor(shadedTriangle.Color, colors);

            yield return new PrefabTriangle(position0, scale0, rotation0, depth, color);
            yield return new PrefabTriangle(position1, scale1, rotation1, depth, color);
        }
    }

    private static int GetThemeColor(Vector3d color, IReadOnlyList<Vector3d> colors)
    {
        int minIndex = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < colors.Count; i++)
        {
            var themeColor = colors[i];
            var distance = Vector3d.Distance(color, themeColor);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }
        
        return minIndex;
    }
    
    private static double GetWindingOrder(Triangle<Vector3d> triangle)
    {
        var a = triangle[0];
        var b = triangle[1];
        var c = triangle[2];
        var ab = b - a;
        var ac = c - a;
        var cross = Vector3d.Cross(ab, ac);
        return cross.Z;
    }
}