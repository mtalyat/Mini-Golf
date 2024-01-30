using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace MiniGolf
{
    internal static class Input
    {
        private static MouseState _lastMouseState;
        public static MouseState LastMouseState => _lastMouseState;
        private static MouseState _currentMouseState;
        public static MouseState CurrentMouseState => _currentMouseState;

        public static void Update(Game game)
        {
            // move current state to last state, get the new state
            _lastMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            // scale the new state to adjust for the resolution size
            _currentMouseState = new MouseState(
                _currentMouseState.X * Constants.RESOLUTION_WIDTH / game.GraphicsDevice.Viewport.Width,
                _currentMouseState.Y * Constants.RESOLUTION_HEIGHT / game.GraphicsDevice.Viewport.Height,
                _currentMouseState.ScrollWheelValue,
                _currentMouseState.LeftButton,
                _currentMouseState.MiddleButton,
                _currentMouseState.RightButton,
                _currentMouseState.XButton1,
                _currentMouseState.XButton2,
                _currentMouseState.HorizontalScrollWheelValue
                );
        }

        private static XnaButtonState GetButtonState(int button, MouseState state)
        {
            switch(button)
            {
                case 0: return state.LeftButton;
                case 1: return state.RightButton;
                case 2: return state.MiddleButton;
                case 3: return state.XButton1;
                case 4: return state.XButton2;
                default: return XnaButtonState.Released;
            }
        }

        public static ButtonState GetMouseButtonState(int button)
        {
            XnaButtonState currentButtonState = GetButtonState(button, _currentMouseState);
            XnaButtonState lastButtonState = GetButtonState(button, _lastMouseState);

            if(currentButtonState == XnaButtonState.Pressed)
            {
                if(lastButtonState == XnaButtonState.Pressed)
                {
                    // still held
                    return ButtonState.Pressed;
                }
                else
                {
                    // starting to be held
                    return ButtonState.Down;
                }
            } else
            {
                if(lastButtonState == XnaButtonState.Pressed)
                {
                    // starting to be released
                    return ButtonState.Up;
                }
                else
                {
                    // still not held
                    return ButtonState.Released;
                }
            }
        }

        public static Vector2 GetMousePosition()
        {
            return new Vector2(_currentMouseState.X, _currentMouseState.Y);
        }

        public static bool ContainsMouse(Hitbox hitbox)
        {
            return hitbox.Contains(new Vector2(_currentMouseState.X, _currentMouseState.Y));
        }
    }
}
