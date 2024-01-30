using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class Hitbox
    {
        /// <summary>
        /// The top left point of the object.
        /// </summary>
        public Vector2 Position;
        
        /// <summary>
        /// The size of the object.
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// The rotation of the object.
        /// </summary>
        public float Rotation;

        public Hitbox(Vector2 position, Vector2 size, float rotation)
        {
            Position = position;
            Size = size;
            Rotation = rotation;
        }

        /// <summary>
        /// Checks if the given position is within this Hitbox.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Contains(Vector2 position)
        {
            // rotate the point to be in the same local space as this object
            Vector2 localPosition = (position - Position).Rotate(MathHelper.ToRadians(-Rotation));

            // assume hitbox is at 0 0 in local space, then check if within
            return
                localPosition.X >= 0 &&
                localPosition.Y >= 0 &&
                localPosition.X <= Size.X &&
                localPosition.Y <= Size.Y;
        }

        /// <summary>
        /// Checks if the given position is within this Hitbox, but a round hitbox. Diameter = Size.X.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool ContainsRound(Vector2 position)
        {
            // rotate the point to be in the same local space as this object
            Vector2 localPosition = (position - Position).Rotate(MathHelper.ToRadians(-Rotation));

            // if the distance to the center is within the radius, it is contained
            return localPosition.DistanceTo(new Vector2(Size.X / 2.0f, Size.Y / 2.0f)) <= Size.X / 2.0f;
        }
    }
}
