using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class TrailObject : SpriteObject
    {
        private readonly Vector2 _start;
        private readonly float _width;
        private Vector2 _end;

        public TrailObject(Vector2 start, float width, Scene scene) : base(new Sprite(scene.Content.Load<Texture2D>("Texture/Trail"), null, new Vector2(0.0f, 0.0f)), scene)
        {
            _start = start;
            _width = width;

            UpdateOrientation();
        }

        private void UpdateOrientation()
        {
            // update end to mouse position
            _end = Input.MousePosition;

            // set rotation to match the line between start and end
            float angle = _start.AngleTo(_end) + MathF.PI * 1.5f;
            LocalRotation = MathHelper.ToDegrees(angle);

            // reposition so the top left is at the appropriate position
            LocalPosition = new Vector2(_width / -2.0f, 0.0f).Rotate(angle) + _start;

            // set the size to be the same width as normal, but the height to match the distance between start and end
            LocalSize = new Vector2(_width, _start.DistanceTo(_end));
        }

        public override void Update(GameTime gameTime)
        {
            UpdateOrientation();

            base.Update(gameTime);
        }
    }
}
