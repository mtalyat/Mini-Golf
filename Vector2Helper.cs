using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal static class Vector2Helper
    {
        public static float DistanceTo(this Vector2 v1, Vector2 v2)
        {
            return Distance(v1, v2);
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            return MathF.Sqrt(MathF.Pow(v2.X - v1.X, 2) + MathF.Pow(v2.Y - v1.Y, 2));
        }

        public static float AngleTo(this Vector2 v1, Vector2 v2)
        {
            return Angle(v1, v2);
        }

        public static float Angle(Vector2 v1, Vector2 v2)
        {
            return MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X) + MathF.PI * 2.0f;
        }

        public static Vector2 DirectionTo(this Vector2 v1, Vector2 v2)
        {
            return Direction(v1, v2);
        }

        public static Vector2 Direction(Vector2 v1, Vector2 v2)
        {
            Vector2 v = v2 - v1;
            v.Normalize();
            return v;
        }

        public static Vector2 FromAngle(float radians)
        {
            return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
        }

        public static Vector2 Rotate(this Vector2 v, float radians)
        {
            // rotate about the origin
            return Rotate(v, Vector2.Zero, radians);
        }

        public static Vector2 Rotate(Vector2 v, Vector2 origin, float radians)
        {
            return FromAngle(Angle(origin, v) + radians) * Distance(v, origin);
        }

        public static Vector2 Lerp(Vector2 v1, Vector2 v2, float amount)
        {
            return new Vector2(MathHelper.Lerp(v1.X, v2.X, amount), MathHelper.Lerp(v1.Y, v2.Y, amount));
        }

        public static Vector2 Snap(this Vector2 v, float snap)
        {
            return new Vector2(MathF.Floor(v.X / snap) * snap, MathF.Floor(v.Y / snap) * snap);
        }

        public static float Magnitude(this Vector2 v)
        {
            return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
        }
    }
}
