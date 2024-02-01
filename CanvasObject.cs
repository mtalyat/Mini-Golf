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
            LocalPosition = -Scene.LocalPosition;

            base.Update(gameTime);
        }
    }
}
