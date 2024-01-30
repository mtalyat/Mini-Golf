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
        private readonly Sprite _sprite;
        public Sprite Sprite => _sprite;

        public Color Color { get; set; } = Color.White;

        protected SpriteBatch SpriteBatch => Scene.SpriteBatch;

        private float _depth = 0.0f;
        public float Depth
        {
            get => _depth;
            set => _depth = Math.Clamp(value, 0.0f, 1.0f);
        }

        public SpriteObject(Sprite sprite, Scene scene) : base(scene)
        {
            _sprite = sprite;
            LocalSize = sprite.Size;
        }

        public SpriteObject(Sprite sprite, Vector2 size, Scene scene) : base(scene)
        {
            _sprite = sprite;
            LocalSize = size;
        }

        public override Vector2 GetGlobalCenter()
        {
            Vector2 size = GetGlobalSize();
            return GetGlobalPosition() - (size * _sprite.Pivot - size / 2.0f).Rotate(MathHelper.ToRadians(GetGlobalRotation()));
        }

        public override Hitbox GetHitbox()
        {
            Vector2 size = GetGlobalSize();
            float rotation = GetGlobalRotation();
            return new Hitbox(GetGlobalPosition() - (size * _sprite.Pivot).Rotate(MathHelper.ToRadians(rotation)), size, rotation);
        }

        public override void Draw(GameTime gameTime)
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

            base.Draw(gameTime);
        }
    }
}
