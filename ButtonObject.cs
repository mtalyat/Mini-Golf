using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class ButtonObject : SpriteObject
    {
        private const int COLOR_COUNT = 3;
        private const int COLOR_NORMAL = 0;
        private const int COLOR_HOVER = 1;
        private const int COLOR_CLICK = 2;
        private readonly Color[] _colors = new Color[COLOR_COUNT]
        {
            Color.White,
            Color.LightGray,
            Color.DarkGray
        };
        public Color NormalColor
        {
            get => _colors[COLOR_NORMAL];
            set => _colors[COLOR_NORMAL] = value;
        }
        public Color HoverColor
        {
            get => _colors[COLOR_HOVER];
            set => _colors[COLOR_HOVER] = value;
        }
        public Color ClickColor
        {
            get => _colors[COLOR_CLICK];
            set => _colors[COLOR_CLICK] = value;
        }
        private int _renderColorIndex = 0;

        private readonly TextObject _textObject;
        public string Text
        {
            get => _textObject.Content;
            set => _textObject.Content = value;
        }
        public Color TextColor
        {
            get => _textObject.Color;
            set => _textObject.Color = value;
        }

        private float _margin = 0.0f;
        public float Margin
        {
            get => _margin;
            set => SetMargin(value);
        }

        public Action<GameObject> OnClick { get; set; } = null;

        public ButtonObject(string text, Sprite sprite, Scene scene, Action<GameObject> onClick = null) : this(text, sprite, 0.0f, scene, onClick) { }

        public ButtonObject(string text, Sprite sprite, float margin, Scene scene, Action<GameObject> onClick = null) : base(sprite, scene)
        {
            _textObject = new TextObject(text, scene);
            OnClick = onClick;

            SetMargin(margin);
            SetDepth(0.9f);
        }

        public override void Initialize()
        {
            Scene.Instantiate(_textObject, this);

            base.Initialize();
        }

        private void UpdateTextObject()
        {
            if (_textObject != null)
            {
                //// get size w/o the margin
                //Vector2 adjustedSize = LocalSize - new Vector2(_margin, _margin) * 2.0f;

                //// get the size of the text itself
                //Vector2 textSize = _textObject.ScaledTextSize;

                //// calculate the offset so that the text is centered in the button
                //Vector2 offset = (LocalSize - textSize) * 0.5f;

                //// adjust text size to match
                //_textObject.LocalPosition = offset;
                //_textObject.LocalSize = adjustedSize;

                _textObject.Pivot = new Vector2(0.5f);
                _textObject.LocalPosition = LocalSize * 0.5f;
                _textObject.LocalSize = LocalSize - new Vector2(_margin * 2.0f);
            }
        }

        protected override void SetDepth(float value)
        {
            // set new depth
            base.SetDepth(MathF.Min(value, 0.9999f));

            // update text to be in front
            _textObject.Depth = Depth + 0.0001f;
        }

        private void SetMargin(float margin)
        {
            _margin = Math.Max(margin, 0.0f);

            UpdateTextObject();
        }

        protected override void SetSize(Vector2 size)
        {
            // set size value
            base.SetSize(size);

            UpdateTextObject();
        }

        public override void Update(GameTime gameTime)
        {
            if(Visible)
            {
                if (Input.ContainsMouse(GetHitbox()))
                {
                    // mouse inside button

                    ButtonState buttonState = Input.GetMouseButtonState(0);
                    if (buttonState <= ButtonState.Down)
                    {
                        // mouse being held down
                        _renderColorIndex = COLOR_CLICK;

                        if (buttonState == ButtonState.Down)
                        {
                            // click detected
                            OnClick?.Invoke(this);
                        }
                    }
                    else
                    {
                        // mouse hovering
                        _renderColorIndex = COLOR_HOVER;
                    }
                }
                else
                {
                    // mouse not over button
                    _renderColorIndex = COLOR_NORMAL;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // update sprite draw color to the render color
            Color = _colors[_renderColorIndex];

            // draw the children (text, etc)
            base.Draw(gameTime);
        }
    }
}
