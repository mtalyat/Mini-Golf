using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class TextObject : SceneObject
    {
        private readonly Text _text;
        public Text Text => _text;
        public string Content
        {
            get => _text.Content;
            set => SetText(value);
        }
        public Vector2 Pivot
        {
            get => _text.Pivot;
            set => _text.Pivot = value;
        }

        private float _scale = 1.0f;
        public Vector2 ScaledTextSize => _text.Size * _scale;

        public Color Color { get; set; } = Color.Black;

        private float _depth = 0.0f;
        public float Depth
        {
            get => _depth;
            set => _depth = Math.Clamp(value, 0.0f, 1.0f);
        }

        public TextObject(string text, Scene scene) : base(scene)
        {
            _text = new Text(text, scene.Content.Load<SpriteFont>("Font/rocks-serif"));
            LocalSize = _text.Size;
        }

        public TextObject(string text, Vector2 size, Scene scene) : this(text, size, Vector2.Zero, scene) { }

        public TextObject(string text, float depth, Scene scene) : this(text, Vector2.Zero, Vector2.Zero, depth, scene) { }

        public TextObject(string text, Vector2 size, float depth, Scene scene) : this(text, size, Vector2.Zero, depth, scene) { }

        public TextObject(string text, Vector2 size, Vector2 pivot, Scene scene) : base(scene)
        {
            _text = new Text(text, scene.Content.Load<SpriteFont>("Font/rocks-serif"), pivot);
            LocalSize = size;
        }

        public TextObject(string text, Vector2 size, Vector2 pivot, float depth, Scene scene) : base(scene)
        {
            _text = new Text(text, scene.Content.Load<SpriteFont>("Font/rocks-serif"), pivot);
            LocalSize = size;
            _depth = depth;
        }

        private void UpdateTextScale()
        {
            // update scale so that it fits within the size the best it can
            // ignore if zero size
            if (_text.Size.X == 0.0f || _text.Size.Y == 0.0f)
            {
                _scale = 0.0f;
            }
            else
            {
                Vector2 size = GetGlobalSize();
                float scaleX = size.X / _text.Size.X;
                float scaleY = size.Y / _text.Size.Y;

                // choose the smaller one so everything fits
                _scale = Math.Min(scaleX, scaleY);
            }
        }

        private void SetText(string text)
        {
            // set the new text
            _text.Content = text;

            UpdateTextScale();
        }

        protected override void SetSize(Vector2 size)
        {
            base.SetSize(size);

            UpdateTextScale();
        }

        protected override void SetScale(Vector2 scale)
        {
            base.SetScale(scale);

            UpdateTextScale();
        }

        // TODO: override GetHitbox

        public override void Draw(GameTime gameTime)
        {
            if (Visible && !string.IsNullOrWhiteSpace(_text.Content))
            {
                Scene.SpriteBatch.DrawString(
                    _text.Font,
                    _text.Content,
                    GetGlobalPosition(),
                    Color,
                    MathHelper.ToRadians(GetGlobalRotation()),
                    _text.Size * _text.Pivot,
                    _scale,
                    SpriteEffects.None,
                    _depth);
            }

            base.Draw(gameTime);
        }
    }
}
