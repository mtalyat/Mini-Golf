using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class EditorScene : Scene
    {
        private LevelInfo _levelInfo;

        public EditorScene(Game game) : base(game)
        {

        }

        protected override void LoadContent()
        {
            _levelInfo = new LevelInfo("Content/Custom/info.txt");

            base.LoadContent();
        }
    }
}
