using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class SelectionObject : SpriteObject
    {
        private readonly Vector2 _start;
        private Vector2 _end;

        public SelectionObject(Vector2 start, Scene scene) : base(new Sprite(scene.Content.Load<Texture2D>("Texture/Selection")), scene)
        {
            _start = start;
            Color = Color.Blue;
            Depth = 1.0f;

            UpdateOrientation();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateOrientation();

            base.Update(gameTime);
        }

        private void UpdateOrientation()
        {
            _end = Input.GetMousePosition();

            LocalPosition = Vector2.Min(_start, _end);
            LocalSize = Vector2.Max(_start, _end) - LocalPosition;
        }
    }
}
