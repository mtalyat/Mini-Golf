using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
            
            //Instantiate(new TextObject())

            base.LoadContent();
        }

        private void Exit()
        {
            MiniGolfGame.LoadScene(SceneType.MainMenu);
        }
    }
}
