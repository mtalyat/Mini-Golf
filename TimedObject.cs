using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class TimedObject : SpriteObject
    {
        private float _timer;

        private readonly Action _onFinish;

        public TimedObject(float time, Action onFinish, Sprite sprite, Scene scene) : base(sprite, scene)
        {
            _timer = time;
            _onFinish = onFinish;
        }

        public override void Update(GameTime gameTime)
        {
            _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // when time expires
            if(_timer <= 0.0f)
            {
                // call action
                _onFinish?.Invoke();

                // destroy self
                Destroy();

                return;
            }

            base.Update(gameTime);
        }
    }
}
