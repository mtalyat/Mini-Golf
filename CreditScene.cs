using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class CreditScene : Scene
    {
        public CreditScene(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            BackgroundSprite = new Sprite(Content.Load<Texture2D>("Texture/MenuBackground"));

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            Instantiate(new ButtonObject("Back", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                Exit();
            })
            {
                Margin = 0.0625f,
                LocalSize = new Vector2(120.0f, 80.0f)
            }, new Vector2(10.0f, Constants.RESOLUTION_HEIGHT - 90.0f));

            LoadCredits();

            base.LoadContent();
        }

        private void LoadCredits()
        {
            const float layoutWidth = Constants.RESOLUTION_WIDTH;

            const float itemWidth = layoutWidth;
            const float itemHeight = 30.0f;

            // create a layout
            LayoutObject layout = Instantiate(new LayoutObject(new Vector2(layoutWidth, Constants.RESOLUTION_HEIGHT), this)
            {
                CellSize = new Vector2(layoutWidth, itemHeight),
                CellOrientation = LayoutObject.Orientation.Vertical,
            }, new Vector2((Constants.RESOLUTION_WIDTH - layoutWidth) * 0.5f, 0.0f));

            // load all lines into the layout in text objects
            string path = Path.Combine(Constants.CONTENT_ROOT_DIRECTORY, "Credits.txt");

            string[] lines = ExternalContent.ReadText(path);

            foreach(string line in lines)
            {
                Instantiate(new TextObject(line, new Vector2(itemWidth, itemHeight), 0.9f, this), layout);
            }

            layout.Refresh();
        }

        private void Exit()
        {
            MiniGolfGame.LoadScene(SceneType.MainMenu);
        }
    }
}
