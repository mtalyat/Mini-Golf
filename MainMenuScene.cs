using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class MainMenuScene : Scene
    {
        public MainMenuScene(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            // TODO: load multiple players
            Session.Players = new List<Player>()
            {
                new Player("Player", Color.White),
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Instantiate(new TextObject("Main menu", new Vector2(Constants.RESOLUTION_WIDTH, 100), new Vector2(0.5f, 0.0f), this), new Vector2(10, 10), 0.0f);

            Instantiate(new ButtonObject("Start", new Sprite(Content.Load<Texture2D>("Texture/UI"), new Rectangle(0, 0, 320, 160), new Vector2(0,0)), 20.0f, this, (GameObject _) =>
            {
                LoadFirstLevel();
            }), new Vector2(400, 400), 0.0f);

            Instantiate(new ButtonObject("Editor", new Sprite(Content.Load<Texture2D>("Texture/UI"), new Rectangle(0, 0, 320, 160), new Vector2(0,0)), 20.0f, this, (GameObject _) =>
            {
                LoadEditor();
            }), new Vector2(400, 600), 0.0f);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void LoadFirstLevel()
        {
            ((MiniGolfGame)Game).LoadLevel(Constants.BUILTIN_WORLD_NAME, 1);
        }

        private void LoadEditor()
        {
            ((MiniGolfGame)Game).LoadScene(SceneType.Editor);
        }
    }
}
