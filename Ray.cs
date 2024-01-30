using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class Ray
    {
        private readonly Vector2 _origin;
        public Vector2 Origin => _origin;
        private readonly Vector2 _direction;
        public Vector2 Direction => _direction;

        public Ray(Vector2 origin, Vector2 direction)
        {
            _origin = origin;
            _direction = direction;
            _direction.Normalize();
        }

        public Vector2 GetPoint(float distance)
        {
            return _origin + _direction * distance;
        }
    }
}
