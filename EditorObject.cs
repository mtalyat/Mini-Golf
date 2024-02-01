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

        private readonly SpriteObject _cover;

        public EditorObject(ObjectType type, Sprite sprite, Scene scene) : base(sprite, scene)
        {
            _type = type;
            _editorScene = (EditorScene)scene;
            _cover = new SpriteObject(new Sprite(scene.Content.Load<Texture2D>("Texture/Selection"), null, Sprite.Pivot), scene);
        }

        public override void Initialize()
        {
            Scene.Instantiate(_cover, Vector2.Zero, 0.0f, this);
            _cover.Color = SELECTED_COLOR;
            _cover.Depth = Depth + 0.0001f;
            _cover.Visible = false;

            base.Initialize();
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

            // if selected, show the cover with the selected color
            _cover.Visible = Selected;
            if(Selected)
            {
                // match size, position and rotation are automatic since this is a child
                _cover.LocalSize = GetGlobalSize();
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

        private bool CheckKey(Keys key, bool snap)
        {
            ButtonState state = Input.GetKeyboardButtonState(key);

            // true when down or pressed, not pressed and snap
            return state == ButtonState.Down || (state == ButtonState.Pressed && !snap);
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

                // only do color if clicking on this specific object
                if(_isDragging)
                {
                    Color = CLICK_COLOR;
                }                

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
                float move = (scale || snap) ? SNAP_SIZE : 1;

                // if not dragging, we can move with WASD if selected
                if (CheckKey(Keys.W, snap))
                {
                    moved = true;
                    LocalPosition += new Vector2(0.0f, -move);
                }
                if (CheckKey(Keys.S, snap))
                {
                    moved = true;
                    LocalPosition += new Vector2(0.0f, move);
                }
                if (CheckKey(Keys.A, snap))
                {
                    moved = true;
                    LocalPosition += new Vector2(-move, 0.0f);
                }
                if (CheckKey(Keys.D, snap))
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
                float move = (scale || snap) ? SNAP_SIZE : 1.0f;

                bool moved = false;

                // we can move with IK if selected
                if (CheckKey(Keys.I, snap))
                {
                    moved = true;
                    LocalSize += new Vector2(0.0f, move);
                }
                if (CheckKey(Keys.K, snap))
                {
                    moved = true;
                    LocalSize += new Vector2(0.0f, -move);
                }
                if (CheckKey(Keys.J, snap))
                {
                    moved = true;
                    LocalSize += new Vector2(-move, 0.0f);
                }
                if (CheckKey(Keys.L, snap))
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
                float move = (scale || snap) ? SNAP_ROTATE : 1.0f;

                bool moved = false;

                // we can move with IK if selected
                if (CheckKey(Keys.Q, snap))
                {
                    moved = true;
                    LocalRotation += -move;
                }
                if (CheckKey(Keys.E, snap))
                {
                    moved = true;
                    LocalRotation += move;
                }
                if(Input.GetKeyboardButtonState(Keys.R) == ButtonState.Down)
                {
                    moved = true;
                    LocalRotation = 0.0f;
                }

                if (moved && snap)
                {
                    LocalRotation = (MathF.Floor(LocalRotation / SNAP_ROTATE) * SNAP_ROTATE) % 360.0f;
                }
            }
        }

        public ObjectData ToObjectData()
        {
            return new ObjectData(GetGlobalPosition(), GetGlobalSize(), GetGlobalRotation());
        }
    }
}
