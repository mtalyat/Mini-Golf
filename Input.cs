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
        public enum MouseButton
        {
            Left,
            Right,
            Middle,
            XButton1,
            XButton2,
        }

        private static MouseState _lastMouseState;
        public static MouseState LastMouseState => _lastMouseState;
        private static MouseState _currentMouseState;
        public static MouseState CurrentMouseState => _currentMouseState;

        public static Vector2 MousePosition => new Vector2(_currentMouseState.X, _currentMouseState.Y);

        private static KeyboardState _lastKeyboardState;
        public static KeyboardState LastKeyboardState => _lastKeyboardState;
        private static KeyboardState _currentKeyboardState;
        public static KeyboardState CurrentKeyboardState => _currentKeyboardState;

        public static void Update(Game game)
        {
            // move current state to last state, get the new state
            _lastMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            _lastKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

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

        public static int GetMouseScrollY() => _currentMouseState.ScrollWheelValue;

        public static int GetMouseScrollX() => _currentMouseState.HorizontalScrollWheelValue;

        public static int GetMouseDeltaScrollY() => _currentMouseState.ScrollWheelValue - _lastMouseState.ScrollWheelValue;

        public static int GetMouseDeltaScrollX() => _currentMouseState.HorizontalScrollWheelValue - _lastMouseState.HorizontalScrollWheelValue;

        private static XnaButtonState GetMouseButtonButtonState(MouseButton button, MouseState state)
        {
            switch(button)
            {
                case MouseButton.Left: return state.LeftButton;
                case MouseButton.Right: return state.RightButton;
                case MouseButton.Middle: return state.MiddleButton;
                case MouseButton.XButton1: return state.XButton1;
                case MouseButton.XButton2: return state.XButton2;
                default: return XnaButtonState.Released;
            }
        }

        public static ButtonState GetMouseButtonState(MouseButton button)
        {
            XnaButtonState currentButtonState = GetMouseButtonButtonState(button, _currentMouseState);
            XnaButtonState lastButtonState = GetMouseButtonButtonState(button, _lastMouseState);

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

        public static ButtonState GetKeyboardButtonState(Keys key)
        {
            if(_currentKeyboardState.IsKeyDown(key))
            {
                if(_lastKeyboardState.IsKeyDown(key))
                {
                    // still held
                    return ButtonState.Pressed;
                }
                else
                {
                    // starting to be held
                    return ButtonState.Down;
                }
            }
            else
            {
                if(_lastKeyboardState.IsKeyDown(key))
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

        public static bool ContainsMouse(Hitbox hitbox)
        {
            return hitbox.Contains(new Vector2(_currentMouseState.X, _currentMouseState.Y));
        }
    }
}
