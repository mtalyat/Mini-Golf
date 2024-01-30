using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class BallCollisionData
    {
        public Vector2 RelativeBallPosition;

        public Vector2 ClampedBallPosition;
    }
}
