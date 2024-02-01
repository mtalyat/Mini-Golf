using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class AimingObject : SpriteObject
    {
        private readonly Vector2 _origin;
        private Vector2 _target;

        public AimingObject(Vector2 origin, Scene scene) : base(new Sprite(scene.Content.Load<Texture2D>("Texture/Aim"), null, new Vector2(0.5f, 0.0f)), scene)
        {
            _origin = origin;

            LocalPosition = origin;
            LocalSize = new Vector2(5.0f, Math.Max(Constants.RESOLUTION_WIDTH, Constants.RESOLUTION_HEIGHT));

            UpdateOrientation();
        }

        private void UpdateOrientation()
        {
            // update end to invert of mouse position
            _target = 2.0f * _origin - Input.MousePosition;

            // set rotation to match the line between start and end
            float angle = _origin.AngleTo(_target) + MathF.PI * 1.5f;
            LocalRotation = MathHelper.ToDegrees(angle);

            // reposition so the top left is at the appropriate position
            //LocalPosition = _origin;

            // set the size to be the same width as normal, but the height to match the distance between start and end
            //LocalSize = new Vector2(1.0f, _origin.DistanceTo(_target));
        }

        public override void Update(GameTime gameTime)
        {
            UpdateOrientation();

            base.Update(gameTime);
        }
    }
}
