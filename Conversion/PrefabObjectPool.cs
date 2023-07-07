using OpenTK.Mathematics;
using PAPrefabToolkit;
using Polyrhythm.Animation;
using Polyrhythm.Data;
using Vector2 = System.Numerics.Vector2;

namespace Polyrhythm.Conversion;

/// <summary>
/// A fixed pool of reusable prefab objects.
/// </summary>
public class PrefabObjectPool
{
    private class StagingPrefabObject
    {
        public List<PositionKeyframe> PositionKeyframes { get; } = new();
        public List<ScaleKeyframe> ScaleKeyframes { get; } = new();
        public List<RotationKeyframe> RotationKeyframes { get; } = new();
        public List<ColorKeyframe> ColorKeyframes { get; } = new();

        public float CalculateStartTime()
        {
            return Math.Min(
                PositionKeyframes[0].Time, 
                Math.Min(
                    ScaleKeyframes[0].Time, 
                    Math.Min(
                        RotationKeyframes[0].Time, 
                        ColorKeyframes[0].Time)));
        }

        public float CalculateKillTime()
        {
            return Math.Max(
                PositionKeyframes[^1].Time, 
                Math.Max(
                    ScaleKeyframes[^1].Time, 
                    Math.Max(
                        RotationKeyframes[^1].Time, 
                        ColorKeyframes[^1].Time)));
        }

        public void AddKeyframes(
            PositionKeyframe positionKeyframe, 
            ScaleKeyframe scaleKeyframe, 
            RotationKeyframe rotationKeyframe, 
            ColorKeyframe colorKeyframe)
        {
            if (PositionKeyframes.Count == 0 || PositionKeyframes[^1].Value != positionKeyframe.Value)
                PositionKeyframes.Add(positionKeyframe);
            if (ScaleKeyframes.Count == 0 || ScaleKeyframes[^1].Value != scaleKeyframe.Value)
                ScaleKeyframes.Add(scaleKeyframe);
            if (RotationKeyframes.Count == 0 || RotationKeyframes[^1].Value != rotationKeyframe.Value)
                RotationKeyframes.Add(rotationKeyframe);
            if (ColorKeyframes.Count == 0 || ColorKeyframes[^1].Value != colorKeyframe.Value)
                ColorKeyframes.Add(colorKeyframe);
        }
    }

    private readonly int capacity;
    private readonly List<StagingPrefabObject> stagingPrefabObjects;

    public PrefabObjectPool(int capacity)
    {
        this.capacity = capacity;
        stagingPrefabObjects = new List<StagingPrefabObject>(capacity);
        for (var i = 0; i < capacity; i++)
            stagingPrefabObjects.Add(new StagingPrefabObject());
    }

    public void AddFrame(float time, IEnumerable<PrefabTriangle> triangles)
    {
        // Sort triangles by depth
        var sortedTriangles = triangles
            .OrderByDescending(x => x.RenderDepth)
            .ToArray();
        
        if (sortedTriangles.Length > capacity)
            throw new InvalidOperationException("Too many triangles for the prefab object pool!");
        
        // Add triangles to staging prefab objects
        for (int i = 0; i < sortedTriangles.Length; i++)
        {
            var triangle = sortedTriangles[i];
            var positionKeyframe = new PositionKeyframe
            {
                Time = time,
                Value = new Vector2((float) triangle.Position.X, (float) triangle.Position.Y),
                Easing = PrefabObjectEasing.Instant
            };
            var scaleKeyframe = new ScaleKeyframe
            {
                Time = time,
                Value = new Vector2((float) triangle.Scale.X, (float) triangle.Scale.Y),
                Easing = PrefabObjectEasing.Instant
            };
            var rotationKeyframe = new RotationKeyframe
            {
                Time = time,
                Value = (float) MathHelper.RadiansToDegrees(triangle.Rotation),
                Easing = PrefabObjectEasing.Instant
            };
            var colorKeyframe = new ColorKeyframe
            {
                Time = time,
                Value = triangle.ThemeColor,
                Easing = PrefabObjectEasing.Instant
            };
            stagingPrefabObjects[i].AddKeyframes(positionKeyframe, scaleKeyframe, rotationKeyframe, colorKeyframe);
        }
        
        // Add inactive keyframes to the rest of staging prefab objects
        for (int i = sortedTriangles.Length; i < capacity; i++)
        {
            var positionKeyframe = new PositionKeyframe
            {
                Time = time,
                Value = Vector2.Zero,
                Easing = PrefabObjectEasing.Instant
            };
            var scaleKeyframe = new ScaleKeyframe
            {
                Time = time,
                Value = Vector2.Zero,
                Easing = PrefabObjectEasing.Instant
            };
            var rotationKeyframe = new RotationKeyframe
            {
                Time = time,
                Value = 0.0f,
                Easing = PrefabObjectEasing.Instant
            };
            var colorKeyframe = new ColorKeyframe
            {
                Time = time,
                Value = 0,
                Easing = PrefabObjectEasing.Instant
            };
            stagingPrefabObjects[i].AddKeyframes(positionKeyframe, scaleKeyframe, rotationKeyframe, colorKeyframe);
        }
    }

    public IEnumerable<PrefabObject> BuildPrefabObjects(Prefab prefab, float duration)
    {
        var step = 160.0f / capacity;
        var renderDepth = 80.0f;
        foreach (var stagingPrefabObject in stagingPrefabObjects)
        {
            var startTime = stagingPrefabObject.CalculateStartTime();
            var killTime = stagingPrefabObject.CalculateKillTime();
            
            if (startTime > killTime)
                continue;

            if (startTime == killTime)
                killTime = duration;

            var prefabObject = prefab.CreateObject("Triangle");
            prefabObject.StartTime = startTime;
            prefabObject.AutoKillType = PrefabObjectAutoKillType.Fixed;
            prefabObject.AutoKillOffset = killTime - startTime;
            prefabObject.RenderDepth = (int) renderDepth;
            prefabObject.PositionParenting = true;
            prefabObject.ScaleParenting = true;
            prefabObject.RotationParenting = true;
            prefabObject.Origin = new Vector2(0.5f, 0.5f);
            prefabObject.ObjectType = PrefabObjectType.Decoration;
            prefabObject.Shape = PrefabObjectShape.Triangle;
            prefabObject.ShapeOption = (int) PrefabTriangleOption.RightAngledSolid;

            foreach (var positionKeyframe in stagingPrefabObject.PositionKeyframes)
            {
                var keyframe = positionKeyframe;
                keyframe.Time -= startTime;
                prefabObject.PositionKeyframes.Add(keyframe);
            }
            
            foreach (var scaleKeyframe in stagingPrefabObject.ScaleKeyframes)
            {
                var keyframe = scaleKeyframe;
                keyframe.Time -= startTime;
                prefabObject.ScaleKeyframes.Add(keyframe);
            }
            
            var lastRotation = 0.0f;
            foreach (var rotationKeyframe in stagingPrefabObject.RotationKeyframes)
            {
                var keyframe = rotationKeyframe;
                var value = keyframe.Value % 360.0f;
                keyframe.Time -= startTime;
                keyframe.Value = value - lastRotation;
                prefabObject.RotationKeyframes.Add(keyframe);
                lastRotation = value;
            }
            
            foreach (var colorKeyframe in stagingPrefabObject.ColorKeyframes)
            {
                var keyframe = colorKeyframe;
                keyframe.Time -= startTime;
                prefabObject.ColorKeyframes.Add(keyframe);
            }

            yield return prefabObject;
            renderDepth -= step;
        }
    }
}