using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    [Flags]
    public enum BehaviorFlags
    {
        None = 0,

        /// <summary>
        /// If static, the object does not move.
        /// If not state, the object can move.
        /// </summary>
        Static = 1 << 0,

        /// <summary>
        /// If collidable, the object can touch other objects.
        /// If not collidable, the object is not involved with collisions.
        /// </summary>
        Collidable = 1 << 1,

        /// <summary>
        /// If solid, no other objects can pass through this one.
        /// If not solid, other objects can pass through this one.
        /// </summary>
        Solid = 1 << 2,

        /// <summary>
        /// If breakable, this object can be broken by moving objects.
        /// If not breakable, this object cannot be broken.
        /// </summary>
        Breakable = 1 << 3,

        /// <summary>
        /// If round, this hitbox is treated as an ellipse.
        /// If not round, this hitbox is treated as a rectangle.
        /// </summary>
        Round = 1 << 4,

        /// <summary>
        /// If hazzard, balls will die when they touch/fully go inside of this object (depending on solid or not).
        /// If not a hazzard, nothing happens.
        /// </summary>
        Hazzard = 1 << 5,
    }

    public static class BehaviorFlagsExtensions
    {
        public static bool HasFlag(this BehaviorFlags flags, BehaviorFlags flag)
        {
            return (flags & flag) != 0;
        }

        public const int BEHAVIOR_FLAG_COUNT = 6;
    }
}
