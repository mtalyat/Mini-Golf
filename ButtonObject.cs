using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        private bool _clicking = false;
        public Action<GameObject> OnClick { get; set; } = null;
        private SoundEffect _downSfx;
        private SoundEffect _upSfx;

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

        protected override void LoadContent()
        {
            _downSfx = Scene.Content.Load<SoundEffect>("Audio/ClickDown");
            _upSfx = Scene.Content.Load<SoundEffect>("Audio/ClickUp");

            base.LoadContent();
        }

        private void UpdateTextObject()
        {
            if (_textObject != null)
            {
                _textObject.Pivot = new Vector2(0.5f);
                _textObject.LocalPosition = LocalSize * 0.5f;
                _textObject.LocalSize = LocalSize - _margin * 2.0f * LocalSize;
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
            _margin = Math.Clamp(margin, 0.0f, 1.0f);

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
                    if (buttonState)
                    {
                        // mouse being held down
                        _renderColorIndex = COLOR_CLICK;

                        if(buttonState == ButtonState.Down)
                        {
                            _downSfx.Play();
                            _clicking = true;
                        }
                    }
                    else if (buttonState == ButtonState.Up && _clicking)
                    {
                        // click detected
                        _upSfx.Play();
                        OnClick?.Invoke(this);
                    }
                    else
                    {
                        // mouse hovering
                        _clicking = false;
                        _renderColorIndex = COLOR_HOVER;
                    }
                }
                else
                {
                    // mouse not over button
                    _clicking = false;
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
