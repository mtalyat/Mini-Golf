using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class CanvasObject : SceneObject
    {
        public CanvasObject(Scene scene) : base(scene)
        {
            LocalSize = new Vector2(Constants.RESOLUTION_WIDTH, Constants.RESOLUTION_HEIGHT);
        }

        public override void Update(GameTime gameTime)
        {
            // always match inverse camera position to give the illusion that the children are sticking with the camera
            LocalPosition = -Scene.LocalPosition / Scene.LocalScale;
            if(Scene.LocalScale.X == 0.0f || Scene.LocalScale.Y == 0.0f)
            {
                LocalScale = Vector2.Zero;
            }
            else
            {
                LocalScale = Vector2.One / Scene.LocalScale;
            }

            base.Update(gameTime);
        }
    }
}
