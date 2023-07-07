using OpenTK.Mathematics;
using Polyrhythm.Data;

namespace Polyrhythm.Util;

// I don't understand how this works, old me wrote this, ask him.
public static class GeometryHelper
{
    private struct AnglePointPair
    {
        public Vector2d Position { get; set; }
        public double Angle { get; set; }

        public AnglePointPair(Vector2d position, double angle)
        {
            Position = position;
            Angle = angle;
        }
    }
    
    public static void GetRightTriangles(Triangle<Vector2d> source, out Triangle<Vector2d> tri0, out Triangle<Vector2d> tri1)
    {
        AnglePointPair[] pairs =
        {
            new(source[0], AngleBetween(source[1] - source[0], source[2] - source[0])), // BAC
            new(source[1], AngleBetween(source[2] - source[1], source[0] - source[1])), // CBA
            new(source[2], AngleBetween(source[1] - source[2], source[0] - source[2])) // BCA
        };

        pairs = pairs.OrderByDescending(x => x.Angle).ToArray();

        var tanB = Math.Tan(pairs[1].Angle);

        var vecBC = pairs[2].Position - pairs[1].Position;
        var BC = vecBC.Length;
        var AH = 2f * TriangleArea(source) / BC;

        var BH = AH / tanB;
        var H = Vector2d.Lerp(pairs[1].Position, pairs[2].Position, BH / BC);

        tri0 = new Triangle<Vector2d>(H, pairs[0].Position, pairs[1].Position);
        tri1 = new Triangle<Vector2d>(H, pairs[0].Position, pairs[2].Position);
    }

    public static void GetPositionScaleRotation(Triangle<Vector2d> triangle, out Vector2d position, out Vector2d scale, out double rotation)
    {
        var AB = triangle[1] - triangle[0];
        var AC = triangle[2] - triangle[0];

        var ABDir = Vector2d.Normalize(AB);
        var ACDir = Vector2d.Normalize(AC);

        position = triangle[0];
        rotation = Math.Atan2(ABDir.Y, ABDir.X); // Rotate the triangle according to AB

        var rotatedACDir = RotateVector2(ACDir, -rotation); // Rotate AC
        scale = new Vector2d(AB.Length, rotatedACDir.Y < 0.0 ? -AC.Length : AC.Length);
    }

    private static Vector2d RotateVector2(Vector2d vec, double radians)
    {
        return new Vector2d(
            vec.X * Math.Cos(radians) - vec.Y * Math.Sin(radians),
            vec.X * Math.Sin(radians) + vec.Y * Math.Cos(radians)
        );
    }

    private static double AngleBetween(Vector2d vec0, Vector2d vec1)
    {
        return Math.Acos(Vector2d.Dot(vec0, vec1) / (vec0.Length * vec1.Length));
    }

    private static double TriangleArea(Triangle<Vector2d> triangle)
    {
        return Math.Abs((triangle[0].X * (triangle[1].Y - triangle[2].Y) + triangle[1].X * (triangle[2].Y - triangle[0].Y) + triangle[2].X * (triangle[0].Y - triangle[1].Y)) / 2f);
    }
}