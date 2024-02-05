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
            Instantiate(new TextObject(Constants.APPLICATION_NAME_UNSAFE, new Vector2(Constants.RESOLUTION_WIDTH, 100), new Vector2(0.5f, 0.0f), this), new Vector2(Constants.RESOLUTION_WIDTH * 0.5f, 0.0f));

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            Instantiate(new ButtonObject("Start", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0,0)), 0.0625f, this, (GameObject _) =>
            {
                LoadFirstLevel();
            }), new Vector2(400, 400));

            Instantiate(new ButtonObject("Levels", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0,0)), 0.0625f, this, (GameObject _) =>
            {
                LoadLevelSelect();
            }), new Vector2(400, 600));

            Instantiate(new ButtonObject("Editor", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0,0)), 0.0625f, this, (GameObject _) =>
            {
                LoadEditor();
            }), new Vector2(400, 800));

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void LoadFirstLevel()
        {
            MiniGolfGame.LoadLevel(Constants.BUILTIN_WORLD_NAME, 1);
        }

        private void LoadLevelSelect()
        {
            MiniGolfGame.LoadScene(SceneType.Select);
        }

        private void LoadEditor()
        {
            MiniGolfGame.LoadScene(SceneType.Editor, "Custom", 1);
        }
    }
}
