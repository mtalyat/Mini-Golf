using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    /// <summary>
    /// A GameObject that always draws a 2D texture.
    /// </summary>
    internal class SpriteObject : SceneObject
    {
        private Sprite _sprite;
        public Sprite Sprite
        {
            get => _sprite;
            set => _sprite = value;
        }

        public Color Color { get; set; } = Color.White;

        protected SpriteBatch SpriteBatch => Scene.SpriteBatch;

        private float _depth = 0.0f;
        public float Depth
        {
            get => _depth;
            set => SetDepth(value);
        }

        public SpriteObject(Sprite sprite, Scene scene) : base(scene)
        {
            _sprite = sprite;
            LocalSize = sprite?.Size ?? Vector2.Zero;
        }

        public SpriteObject(Sprite sprite, Vector2 size, Scene scene) : base(scene)
        {
            _sprite = sprite;
            LocalSize = size;
        }

        public override Vector2 GetGlobalCenter()
        {
            return base.GetGlobalCenter() + (-GetGlobalSize() * _sprite?.Pivot ?? Vector2.Zero).Rotate(MathHelper.ToRadians(GetGlobalRotation()));
        }

        public override Hitbox GetHitbox()
        {
            Vector2 size = GetGlobalSize();
            float rotation = GetGlobalRotation();
            return new Hitbox(GetGlobalPosition() - (size * _sprite?.Pivot ?? Vector2.Zero).Rotate(MathHelper.ToRadians(rotation)), size, rotation);
        }

        public override void Draw(GameTime gameTime)
        {
            if(Visible && _sprite != null)
            {
                Vector2 position = GetGlobalPosition();
                Vector2 size = GetGlobalSize();
                float rotation = GetGlobalRotation();

                SpriteBatch.Draw(
                    _sprite.Texture,
                    new Rectangle((int)MathF.Floor(position.X), (int)MathF.Floor(position.Y), (int)MathF.Floor(size.X), (int)MathF.Floor(size.Y)),
                    _sprite.Slice,
                    Color,
                    MathHelper.ToRadians(rotation),
                    _sprite.Size * _sprite.Pivot,
                    SpriteEffects.None,
                    _depth);
            }            

            base.Draw(gameTime);
        }

        protected virtual void SetDepth(float value)
        {
            _depth = Math.Clamp(value, 0.0f, 1.0f);
        }
    }
}
