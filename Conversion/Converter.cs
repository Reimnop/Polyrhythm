using OpenTK.Mathematics;
using PAPrefabToolkit;
using PAThemeToolkit;
using Polyrhythm.Data;
using Polyrhythm.Rendering;
using Polyrhythm.Rendering.Shader;
using Polyrhythm.Util;
using Vector2 = System.Numerics.Vector2;

namespace Polyrhythm.Conversion;

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

    public Prefab CreatePrefab(InitializeCallback? initializeCallback = null, UpdateCallback? updateCallback = null)
    {
        var model = configuration.InputModel;
        var animationHandler = new AnimationHandler(model);
        initializeCallback?.Invoke(animationHandler);

        // TODO: Don't hardcode camera
        var camera = new PerspectiveCamera();
        camera.Position = Vector3d.UnitZ * 2.0;
        var pipeline = new Pipeline(model, animationHandler, camera, VertexShader.Factory, TriangleShader.Factory);
        
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
        
        // Turn frames into prefab
        // TODO: Don't generate a new set of triangles every single frame
        int objectsCount = 1;
        var prefab = new Prefab();
        var parent = prefab.CreateObject("Parent");
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
            Value = Vector2.One * 10.0f
        });
        parent.RotationKeyframes.Add(new RotationKeyframe());
        parent.ColorKeyframes.Add(new ColorKeyframe());
        for (int i = 0; i < frames.Count; i++)
        {
            var startTime = i * configuration.FrameDuration;
            var killTime = Math.Min(startTime + configuration.FrameDuration, configuration.Duration);
            
            if (startTime >= killTime) // Early exit
                break;
            
            var frame = frames[i];
            foreach (var prefabTriangle in EnumeratePrefabTriangles(frame, configuration.InputTheme))
            {
                var prefabObject = prefab.CreateObject($"Frame {i}");
                prefabObject.StartTime = (float) startTime;
                prefabObject.AutoKillType = PrefabObjectAutoKillType.Fixed;
                prefabObject.AutoKillOffset = (float) (killTime - startTime);
                prefabObject.RenderDepth = prefabTriangle.RenderDepth;
                prefabObject.PositionParenting = true;
                prefabObject.ScaleParenting = true;
                prefabObject.RotationParenting = true;
                prefabObject.Origin = new Vector2(0.5f, 0.5f);
                prefabObject.ObjectType = PrefabObjectType.Decoration;
                prefabObject.Shape = PrefabObjectShape.Triangle;
                prefabObject.ShapeOption = (int) PrefabTriangleOption.RightAngledSolid;
                prefabObject.PositionKeyframes.Add(new PositionKeyframe
                {
                    Value = new Vector2((float) prefabTriangle.Position.X, (float) prefabTriangle.Position.Y)
                });
                prefabObject.ScaleKeyframes.Add(new ScaleKeyframe
                {
                    Value = new Vector2((float) prefabTriangle.Scale.X, (float) prefabTriangle.Scale.Y)
                });
                prefabObject.RotationKeyframes.Add(new RotationKeyframe
                {
                    Value = (float) MathHelper.RadiansToDegrees(prefabTriangle.Rotation)
                });
                prefabObject.ColorKeyframes.Add(new ColorKeyframe
                {
                    Value = prefabTriangle.ThemeColor
                });
                
                prefabObject.SetParent(parent);
                objectsCount++;
            }
        }
        
        Console.WriteLine("Created {0} objects", objectsCount);
        
        return prefab;
    }

    private static IEnumerable<PrefabTriangle> EnumeratePrefabTriangles(PrefabFrame frame, Theme theme)
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
            var renderDepth = CalculateRenderDepth(depth);
            var color = GetThemeColor(shadedTriangle.Color, theme);

            yield return new PrefabTriangle(position0, scale0, rotation0, renderDepth, color);
            yield return new PrefabTriangle(position1, scale1, rotation1, renderDepth, color);
        }
    }

    private static int CalculateRenderDepth(double depth)
    {
        return (int) ((depth * 2.0 - 1.0) * 60.0);
    }

    private static int GetThemeColor(Vector3d color, Theme theme)
    {
        int minIndex = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < theme.Objects.Length; i++)
        {
            var themeColor = theme.Objects[i];
            var themeColorVec3 = new Vector3d(themeColor.R, themeColor.G, themeColor.B);
            var distance = Vector3d.Distance(color, themeColorVec3);
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