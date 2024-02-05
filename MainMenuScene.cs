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
            Instantiate(new TextObject(Constants.APPLICATION_NAME_UNSAFE, new Vector2(Constants.RESOLUTION_WIDTH, 150), new Vector2(0.5f, 0.0f), this), new Vector2(Constants.RESOLUTION_WIDTH * 0.5f, 50.0f));

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            const float panelDepth = 0.9f;
            const float panelItemDepth = panelDepth + 0.001f;
            const float panelSpacing = 20.0f;
            const float panelWidth = 440.0f;
            const float panelItemWidth = panelWidth - panelSpacing * 2.0f;
            Vector2 panelItemSize = new(panelItemWidth, 180.0f);

            Instantiate(new SpriteObject(new Sprite(uiTexture, Constants.UI_BACKGROUND), new Vector2(panelWidth, Constants.RESOLUTION_HEIGHT), panelDepth, this));

            Instantiate(new ButtonObject("Start", new Sprite(uiTexture, Constants.UI_BUTTON), 0.0625f, this, (GameObject _) =>
            {
                LoadFirstLevel();
            })
            {
                Depth = panelItemDepth,
                LocalSize = panelItemSize,
            }, new Vector2(panelSpacing, panelSpacing));

            Instantiate(new ButtonObject("Levels", new Sprite(uiTexture, Constants.UI_BUTTON), 0.0625f, this, (GameObject _) =>
            {
                LoadLevelSelect();
            })
            {
                Depth = panelItemDepth,
                LocalSize = panelItemSize,
            }, new Vector2(panelSpacing, panelSpacing * 2.0f + panelItemSize.Y * 1.0f));

            Instantiate(new ButtonObject("Editor", new Sprite(uiTexture, Constants.UI_BUTTON), 0.0625f, this, (GameObject _) =>
            {
                LoadEditor();
            })
            {
                Depth = panelItemDepth,
                LocalSize = panelItemSize,
            }, new Vector2(panelSpacing, panelSpacing * 3.0f + panelItemSize.Y * 2.0f));

            Instantiate(new ButtonObject("Credits", new Sprite(uiTexture, Constants.UI_BUTTON), 0.0625f, this, (GameObject _) =>
            {
                LoadCredits();
            })
            {
                Depth = panelItemDepth,
                LocalSize = panelItemSize,
            }, new Vector2(panelSpacing, panelSpacing * 4.0f + panelItemSize.Y * 3.0f));

            Instantiate(new ButtonObject("Exit", new Sprite(uiTexture, Constants.UI_BUTTON), 0.0625f, this, (GameObject _) =>
            {
                // close game
                Game.Exit();
            })
            {
                Depth = panelItemDepth,
                LocalSize = panelItemSize,
            }, new Vector2(panelSpacing, panelSpacing * 5.0f + panelItemSize.Y * 4.0f));

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

        private void LoadCredits()
        {
            MiniGolfGame.LoadScene(SceneType.Credit);
        }
    }
}
