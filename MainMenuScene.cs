﻿using Microsoft.Xna.Framework;
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
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Instantiate(new TextObject("Main menu", new Vector2(Constants.RESOLUTION_WIDTH, 100), new Vector2(0.5f, 0.0f), this), new Vector2(10, 10), 0.0f);

            Instantiate(new ButtonObject("Start", new Sprite(Content.Load<Texture2D>("Texture/UI"), new Rectangle(0, 0, 320, 160), new Vector2(0,0)), 20.0f, this, (GameObject _) =>
            {
                Play();
            }), new Vector2(400, 400), 0.0f);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void Play()
        {
            // TODO: load players
            Session.Players = new List<Player>()
            {
                new Player("Player", Color.White),
            };

            MiniGolfGame.Instance.LoadScene(SceneType.Level, 1);
        }
    }
}
