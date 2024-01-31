using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class ThwackObject : SpriteObject
    {
        private float _timer;

        public ThwackObject(Vector2 start, Vector2 end, float width, Scene scene) : base(new Sprite(scene.Content.Load<Texture2D>("Texture/Thwack"), null, new Vector2(0.5f, 0.0f)), scene)
        {
            LocalPosition = start;
            LocalRotation = MathHelper.ToDegrees(start.AngleTo(end) + MathF.PI * 1.5f);
            LocalSize = new Vector2(width, start.DistanceTo(end));

            _timer = Constants.BALL_THWACK_TIME;
        }

        public override void Update(GameTime gameTime)
        {
            // increment timer
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // if timer over, stop
            if(_timer <= 0.0f)
            {
                Destroy();
                return;
            }

            // timer not over: update color alpha so it fades over time
            float percent = _timer / Constants.BALL_THWACK_TIME;
            Color = new Color(Color.R, Color.G, Color.B, percent);

            base.Update(gameTime);
        }
    }
}
