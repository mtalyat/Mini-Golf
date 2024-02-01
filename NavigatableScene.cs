using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    // a scene that can be navigated using the middle mouse button/scroll
    internal abstract class NavigatableScene : Scene
    {
        private bool _move = false;
        private Vector2 _moveOffset;
        private float _targetZoomScale = 1.0f;
        protected bool CanMoveCamera { get; set; } = true;

        protected virtual ButtonState CameraMoveButtonState
        {
            get => Input.GetMouseButtonState(Input.MouseButton.Middle);
        }

        public NavigatableScene(Game game) : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            // drag camera around
            ButtonState buttonState = CameraMoveButtonState;
            if (buttonState == ButtonState.Down)
            {
                _move = true;
                _moveOffset = LocalPosition - Input.MousePosition;
            }

            if (_move)
            {
                LocalPosition = Input.MousePosition + _moveOffset;
            }

            // stop dragging camera around
            if (buttonState == ButtonState.Up)
            {
                _move = false;
            }

            // scroll/zoom in and out if no modifiers being pressed
            if (!Input.GetKeyboardAny(Keys.LeftControl, Keys.LeftShift))
            {
                int scroll = Input.GetMouseDeltaScrollY();

                if (scroll > 0)
                {
                    // zoom in
                    _targetZoomScale = LocalScale.X * Constants.CAMERA_ZOOM_FACTOR;
                }

                if (scroll < 0)
                {
                    // zoom out
                    _targetZoomScale = LocalScale.X / Constants.CAMERA_ZOOM_FACTOR;
                }
            }

            // if not at target zoom, zoom to it
            if(LocalScale.X != _targetZoomScale)
            {
                // zoom so that the mouse is the focal point
                Vector2 mousePosition = Input.MousePosition;
                Vector2 oldPosition = CameraPosition - mousePosition;
                float oldZoomScale = LocalScale.X;

                // lerp to new scale
                float newZoomScale = MathHelper.Lerp(oldZoomScale, _targetZoomScale, Constants.CAMERA_ZOOM_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds);
                LocalScale = new Vector2(newZoomScale);

                // move center to adjust
                CameraPosition = (oldPosition / oldZoomScale) * newZoomScale + mousePosition;
            }

            base.Update(gameTime);
        }
    }
}
