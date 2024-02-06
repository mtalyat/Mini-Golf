using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class MainMenuScene : Scene
    {
        private Texture2D _musicOnTexture;
        private Texture2D _musicOffTexture;
        private Texture2D _soundOnTexture;
        private Texture2D _soundOffTexture;

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
            CreateSoundButtons();

            BackgroundSprite = new Sprite(Content.Load<Texture2D>("Texture/MenuBackground"));

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

        #region Sound

        private void CreateSoundButtons()
        {
            _musicOnTexture = Content.Load<Texture2D>("Texture/MusicOn");
            _musicOffTexture = Content.Load<Texture2D>("Texture/MusicOff");
            _soundOnTexture = Content.Load<Texture2D>("Texture/SoundOn");
            _soundOffTexture = Content.Load<Texture2D>("Texture/SoundOff");

            const float spacing = 20.0f;
            const float size = 80.0f;

            // music
            ButtonObject musicButton = Instantiate(new ButtonObject(string.Empty, new Sprite(_musicOnTexture), this, (GameObject sender) => ToggleMusic((ButtonObject)sender))
            {
                LocalSize = new Vector2(size),
            }, new Vector2(Constants.RESOLUTION_WIDTH - (spacing + size) * 2.0f, spacing));

            // sound
            ButtonObject soundButton = Instantiate(new ButtonObject(string.Empty, new Sprite(_soundOnTexture), this, (GameObject sender) => ToggleSound((ButtonObject)sender))
            {
                LocalSize = new Vector2(size),
            }, new Vector2(Constants.RESOLUTION_WIDTH - (spacing + size), spacing));

            // im lazy so just toggle twice to update the button sprites appropriately
            ToggleMusic(musicButton);
            ToggleMusic(musicButton);
            ToggleSound(soundButton);
            ToggleSound(soundButton);
        }

        private void ToggleMusic(ButtonObject button)
        {
            if (MediaPlayer.IsMuted)
            {
                MediaPlayer.IsMuted = false;
                button.Sprite = new Sprite(_musicOnTexture);
            }
            else
            {
                MediaPlayer.IsMuted = true;
                button.Sprite = new Sprite(_musicOffTexture);
            }
        }

        private void ToggleSound(ButtonObject button)
        {
            if (SoundEffect.MasterVolume > 0.0f)
            {
                SoundEffect.MasterVolume = 0.0f;
                button.Sprite = new Sprite(_soundOffTexture);
            }
            else
            {
                SoundEffect.MasterVolume = 1.0f;
                button.Sprite = new Sprite(_soundOnTexture);
            }
        }

        #endregion

        private void LoadFirstLevel()
        {
            MiniGolfGame.LoadLevel(Constants.BUILTIN_WORLD_NAME, 1, SceneType.MainMenu);
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
