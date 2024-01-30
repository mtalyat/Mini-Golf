using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
    internal class SceneObject : GameObject
    {
        private readonly Scene _scene;
        public Scene Scene => _scene;

        public SceneObject(Scene scene) : base(scene.Game)
        {
            _scene = scene;

            // add to scene
            SetParent(scene);
        }

        public override void Destroy()
        {
            // destroy self as normal
            base.Destroy();

            // also remove self from scene
            SetParent(null);
        }
    }
}
