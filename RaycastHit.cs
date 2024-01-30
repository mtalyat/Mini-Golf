using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class RaycastHit
    {
        public GameObject GameObject;

        public Ray Ray;

        public float Distance;

        public Vector2 Normal;

        /// <summary>
        /// Miss constructor.
        /// </summary>
        /// <param name="ray"></param>
        public RaycastHit(Ray ray)
        {
            Ray = ray;
        }

        /// <summary>
        /// Hit constructor.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="gameObject"></param>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        public RaycastHit(Ray ray, GameObject gameObject, float distance, Vector2 normal)
        {
            GameObject = gameObject;
            Ray = ray;
            Distance = distance;
            Normal = normal;
        }

        public static implicit operator bool(RaycastHit r) => r.GameObject != null;
    }
}
