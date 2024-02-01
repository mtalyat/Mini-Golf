using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class EditorObject : SpriteObject
    {
        private const float SNAP_MOVE = 20.0f;
        private const float SNAP_SIZE = 10.0f;
        private const float SNAP_ROTATE = 15.0f;

        private static readonly Color NORMAL_COLOR = Color.White;
        private static readonly Color HOVER_COLOR = Color.LightGray;
        private static readonly Color CLICK_COLOR = Color.Gray;
        private static readonly Color SELECTED_COLOR = Color.Yellow;

        private readonly ObjectType _type;
        public ObjectType Type => _type;

        private bool _isDragging = false;
        private bool _movedWhileDragging = false;

        private Vector2 _offset;

        public bool Selected { get; set; }

        private readonly EditorScene _editorScene;

        public EditorObject(ObjectType type, Sprite sprite, Scene scene) : base(sprite, scene)
        {
            _type = type;
            _editorScene = (EditorScene)scene;
        }

        public override void Update(GameTime gameTime)
        {
            if (UpdateOther())
            {
                return;
            }

            bool snap = Input.GetKeyboardButtonState(Keys.LeftControl) <= ButtonState.Down;
            bool scale = Input.GetKeyboardButtonState(Keys.LeftShift) <= ButtonState.Down;

            // reset color
            Color = NORMAL_COLOR;

            UpdatePosition(snap, scale);
            UpdateSize(snap, scale);
            UpdateRotation(snap, scale);

            // if selected, use selected color instead
            if (Selected)
            {
                Color = SELECTED_COLOR;
            }

            base.Update(gameTime);
        }

        private bool UpdateOther()
        {
            if (Selected && Input.GetKeyboardButtonState(Keys.Delete) == ButtonState.Down)
            {
                Destroy();
                return true;
            }

            return false;
        }

        private void UpdatePosition(bool snap, bool scale)
        {
            ButtonState mouseButtonState = Input.GetMouseButtonState(Input.MouseButton.Left);

            bool containsMouse = Input.ContainsMouse(GetHitbox());

            bool holdingModifierKey = Input.GetKeyboardButtonState(Keys.LeftShift) <= ButtonState.Down;

            if (containsMouse)
            {
                Color = HOVER_COLOR;
            }

            if (mouseButtonState == ButtonState.Down)
            {
                if(containsMouse)
                {
                    _editorScene.DoNotDragSelect();
                }

                if(containsMouse || Selected)
                {
                    // if not selected, unselect all others
                    if (!Selected && !holdingModifierKey)
                    {
                        _editorScene.SetAllSelected(false);
                    }

                    // start dragging
                    if (containsMouse)
                    {
                        _isDragging = true;
                        _movedWhileDragging = false;
                    }
                    _offset = LocalPosition - Input.GetMousePosition();
                }
            }

            bool moved = false;

            // move object to mouse if dragging
            if (_isDragging || (_editorScene.UniversalDrag && Selected))
            {
                moved = true;

                Color = CLICK_COLOR;

                // move to mouse + offset
                Vector2 newPosition = Input.GetMousePosition() + _offset;
                if(LocalPosition !=  newPosition)
                {
                    _movedWhileDragging = true;
                    LocalPosition = newPosition;
                }
            }

            if (!_isDragging && Selected)
            {
                float move = scale ? SNAP_SIZE : 1;

                // if not dragging, we can move with WASD if selected
                if (Input.GetKeyboardButtonState(Keys.W) <= ButtonState.Down)
                {
                    moved = true;
                    LocalPosition += new Vector2(0.0f, -move);
                }
                if (Input.GetKeyboardButtonState(Keys.S) <= ButtonState.Down)
                {
                    moved = true;
                    LocalPosition += new Vector2(0.0f, move);
                }
                if (Input.GetKeyboardButtonState(Keys.A) <= ButtonState.Down)
                {
                    moved = true;
                    LocalPosition += new Vector2(-move, 0.0f);
                }
                if (Input.GetKeyboardButtonState(Keys.D) <= ButtonState.Down)
                {
                    moved = true;
                    LocalPosition += new Vector2(move, 0.0f);
                }
            }

            // snap only if moved
            if (moved && snap)
            {
                LocalPosition = LocalPosition.Snap(SNAP_MOVE);
            }

            if (mouseButtonState == ButtonState.Up && _isDragging)
            {
                // done dragging
                _isDragging = false;

                // if did not move, it was a click
                if(!_movedWhileDragging)
                {
                    if(holdingModifierKey)
                    {
                        // if shift being held, add or remove from selection
                        Selected = !Selected;
                    }
                    else
                    {
                        // only select this one
                        _editorScene.SetAllSelected(false);
                        Selected = true;
                    }
                }
            }
        }

        private void UpdateSize(bool snap, bool scale)
        {
            if (Selected)
            {
                float move = scale ? SNAP_SIZE : 1.0f;

                bool moved = false;

                // we can move with IK if selected
                if (Input.GetKeyboardButtonState(Keys.I) <= ButtonState.Down)
                {
                    moved = true;
                    LocalSize += new Vector2(0.0f, move);
                }
                if (Input.GetKeyboardButtonState(Keys.K) <= ButtonState.Down)
                {
                    moved = true;
                    LocalSize += new Vector2(0.0f, -move);
                }
                if (Input.GetKeyboardButtonState(Keys.J) <= ButtonState.Down)
                {
                    moved = true;
                    LocalSize += new Vector2(-move, 0.0f);
                }
                if (Input.GetKeyboardButtonState(Keys.L) <= ButtonState.Down)
                {
                    moved = true;
                    LocalSize += new Vector2(move, 0.0f);
                }

                if (moved && snap)
                {
                    LocalSize = LocalSize.Snap(SNAP_SIZE);
                }
            }
        }

        private void UpdateRotation(bool snap, bool scale)
        {
            if (Selected)
            {
                float move = scale ? SNAP_ROTATE : 1.0f;

                bool moved = false;

                // we can move with IK if selected
                if (Input.GetKeyboardButtonState(Keys.Q) <= ButtonState.Down)
                {
                    moved = true;
                    LocalSize += new Vector2(0.0f, move);
                }
                if (Input.GetKeyboardButtonState(Keys.E) <= ButtonState.Down)
                {
                    moved = true;
                    LocalSize += new Vector2(0.0f, -move);
                }

                if (moved && snap)
                {
                    LocalSize = LocalSize.Snap(SNAP_SIZE);
                }
            }
        }

        public ObjectData ToObjectData()
        {
            return new ObjectData(GetGlobalPosition(), GetGlobalSize(), GetGlobalRotation());
        }
    }
}
