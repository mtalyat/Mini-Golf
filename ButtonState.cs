using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal enum ButtonState
    {
        /// <summary>
        /// The button is being pressed.
        /// </summary>
        Pressed,

        /// <summary>
        /// The button pressed down for the first frame.
        /// </summary>
        Down,
        
        /// <summary>
        /// The button released for the first frame.
        /// </summary>
        Up,

        /// <summary>
        /// The button is not being pressed.
        /// </summary>
        Released
    }
}
