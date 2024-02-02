using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MiniGolf.BallObject;

namespace MiniGolf
{
    internal struct ButtonState
    {
        //    /// <summary>
        //    /// The button is being pressed.
        //    /// </summary>
        public const int Pressed = 0;
        //    /// <summary>
        //    /// The button pressed down for the first frame.
        //    /// </summary>
        public const int Down = 1;
        //    /// <summary>
        //    /// The button released for the first frame.
        //    /// </summary>
        public const int Up = 2;
        //    /// <summary>
        //    /// The button is not being pressed.
        //    /// </summary>
        public const int Released = 3;

        private readonly int _state;
        public readonly int State => _state;

        public ButtonState(int state)
        {
            _state = state;
        }

        public static implicit operator int(ButtonState state) => state._state;
        public static implicit operator ButtonState(int state) => new(state);

        public static implicit operator bool(ButtonState state) => state._state <= Down;

        public readonly ButtonState Combine(ButtonState other)
        {
            return _state < other._state ? this : other;
        }

        public override readonly string ToString()
        {
            return _state switch
            {
                Pressed => "Pressed",
                Down => "Down",
                Up => "Up",
                Released => "Released",
                _ => string.Empty,
            };
        }
    }
}
