using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class Text
    {
        private string _content;
        public string Content
        {
            get => _content;
            set => SetContent(value);
        }

        public SpriteFont Font { get; set; }

        public Vector2 Size { get; private set; }

        public Vector2 Pivot { get; set; }

        public Text(string content, SpriteFont font, Vector2? pivot = null)
        {
            Font = font;
            SetContent(content);
            Pivot = pivot ?? Vector2.Zero;
        }

        private void SetContent(string content)
        {
            _content = content;
            Size = Font.MeasureString(content);
        }
    }
}
