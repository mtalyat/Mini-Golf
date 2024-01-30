using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
    internal class Sprite
    {
        private readonly Texture2D _texture;
        public Texture2D Texture => _texture;

        private readonly Rectangle? _slice;
        public Rectangle? Slice => _slice;

        public Vector2 Pivot { get; set; }

        public Vector2 Size => GetSize();

        /// <summary>
        /// Creates a Sprite that draws part of a Texture.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="slice">The part of the Texture to draw, or null if all of it is to be drawn.</param>
        public Sprite(Texture2D texture, Rectangle? slice = null, Vector2? pivot = default)
        {
            _texture = texture;
            _slice = slice;
            Pivot = pivot ?? Vector2.Zero;
        }

        private Vector2 GetSize()
        {
            if(_slice == null)
            {
                return new Vector2(_texture.Width, _texture.Height);
            }
            else
            {
                return new Vector2(_slice.Value.Width, _slice.Value.Height);
            }
        }
    }
}
