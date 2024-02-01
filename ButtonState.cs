using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    // TODO: make this a struct
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

    internal static class ButtonStateExtensions
    {
        public static ButtonState Combine(this ButtonState state1, ButtonState state2)
        {
            return state1 < state2 ? state1 : state2;
        }
    }
}
