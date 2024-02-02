using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal struct Bounds
    {
        public Vector2 Min;
        public Vector2 Max;

        public readonly Vector2 Position => Min;
        public readonly Vector2 Size => Max - Min;

        public Bounds()
        {
            Min = Vector2.Zero;
            Max = Vector2.Zero;
        }

        public Bounds(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }
    }
}
