using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MiniGolf
{
    internal enum ObjectType
    {
        /// <summary>
        /// It could be anything.
        /// </summary>
        Generic,

        /// <summary>
        /// An object that can be hit by the player.
        /// </summary>
        Ball,

        /// <summary>
        /// The end goal.
        /// </summary>
        Hole,

        /// <summary>
        /// The spawn point for the balls.
        /// </summary>
        Start,

        /// <summary>
        /// A solid object that cannot be passed through.
        /// </summary>
        Wall,

        /// <summary>
        /// A solid object that can be bounced off of or broken, using the correct ball.
        /// </summary>
        WallDamaged,

        /// <summary>
        /// A wall end.
        /// </summary>
        Wall1,

        /// <summary>
        /// A wall L intersection (corner).
        /// </summary>
        Wall2,

        /// <summary>
        /// A wall T intersection.
        /// </summary>
        Wall3,

        /// <summary>
        /// A wall + intersection.
        /// </summary>
        Wall4,

        /// <summary>
        /// A space that will push the ball in a certain direction.
        /// </summary>
        Slope,

        /// <summary>
        /// The ball is pushed away from the center.
        /// </summary>
        Hill,

        /// <summary>
        /// An inverted hill. The ball will roll towards the center.
        /// </summary>
        Valley,

        /// <summary>
        /// A space that will slow the ball.
        /// </summary>
        Sand,

        /// <summary>
        /// A space that will kill the ball.
        /// </summary>
        Water,

        /// <summary>
        /// A solid object that can be broken, using the correct balls.
        /// </summary>
        Box,

        /// <summary>
        /// A solid object that cannot be broken.
        /// </summary>
        Crate,

        ///// <summary>
        ///// A wall that can spin.
        ///// </summary>
        //RotatingWall,

        ///// <summary>
        ///// A wall that can move.
        ///// </summary>
        //MovingWall,
    }
}
