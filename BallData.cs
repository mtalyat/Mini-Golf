using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal struct BallData
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public GameObject Target { get; set; }

        public BallData(Vector2 position, float rotation = 0.0f, GameObject target = null)
        {
            Position = position;
            Rotation = rotation;
            Target = target;
        }
    }
}
