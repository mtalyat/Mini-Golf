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

            // offset by center
            localPosition -= Size / 2.0f;

            // if the distance to the center is within the radius, it is contained
            //return localPosition.DistanceTo(new Vector2(Size.X / 2.0f, Size.Y / 2.0f)) <= Size.X / 2.0f;
            // https://math.stackexchange.com/questions/76457/check-if-a-point-is-within-an-ellipse#:~:text=The%20region%20(disk)%20bounded%20by,it%20is%20outside%20the%20ellipse
            // assume h and k are 0 and 0, since the position is localized
            return (localPosition.X * localPosition.X) / MathF.Pow(Size.X / 2.0f, 2.0f) +
                (localPosition.Y * localPosition.Y) / MathF.Pow(Size.Y / 2.0f, 2.0f) <= 1.0f;
        }

        public Vector2[] GetCorners()
        {
            return new Vector2[4]
            {
                Position,
                Position + new Vector2(Size.X, 0.0f).Rotate(MathHelper.ToRadians(-Rotation)),
                Position + new Vector2(0.0f, Size.Y).Rotate(MathHelper.ToRadians(-Rotation)),
                Position + new Vector2(Size.X, Size.Y).Rotate(MathHelper.ToRadians(-Rotation))
            };
        }

        public (Vector2, Vector2) GetMinMax()
        {
            // top left is min of X and Y
            Vector2[] corners = GetCorners();

            Vector2 topLeft = new(float.MaxValue, float.MaxValue);
            Vector2 bottomRight = new(float.MinValue, float.MinValue);

            foreach (Vector2 corner in corners)
            {
                if (corner.X < topLeft.X) topLeft.X = corner.X;
                if (corner.Y < topLeft.Y) topLeft.Y = corner.Y;

                if(corner.X > bottomRight.X) bottomRight.X = corner.X;
                if(corner.Y > bottomRight.Y) bottomRight.Y = corner.Y;
            }

            return (topLeft, bottomRight);
        }
    }
}
