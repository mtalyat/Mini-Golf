using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Formats.Asn1.AsnWriter;
using XnaButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace MiniGolf
{
    public class MiniGolfGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _targetBatch;
        private RenderTarget2D _target;
        private Scene _currentScene;
        private Scene _nextScene;

        private Song _song;

        public MiniGolfGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = Constants.CONTENT_ROOT_DIRECTORY;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // initialize the window
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowAltF4 = true;
            ResetTitle();

            // create target so the game can scale with the window
            _targetBatch = new SpriteBatch(GraphicsDevice);
            _target = new RenderTarget2D(GraphicsDevice, Constants.RESOLUTION_WIDTH, Constants.RESOLUTION_HEIGHT);

            // create the level, the scene does the rest
            LoadScene(SceneType.MainMenu);

            base.Initialize();
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            
        }

        protected override void LoadContent()
        {
            _song = Content.Load<Song>("Audio/Golf");
            // TODO: sound settings
            MediaPlayer.Play(_song);
            MediaPlayer.IsRepeating = true;

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            MediaPlayer.Stop();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // update the input system
            Input.Update(this);

            // change current scene to the next scene if needed
            UpdateCurrentScene();

            // update the scene
            base.Update(gameTime);
        }

        private void UpdateCurrentScene()
        {
            if (_nextScene != null)
            {
                // unload current scene if any
                if (_currentScene != null)
                {
                    Components.Remove(_currentScene);
                    _currentScene.Destroy();
                }

                // set the new scene to the next scene
                _currentScene = _nextScene;
                _nextScene = null;

                // reset title, if the scene changed it
                ResetTitle();

                // load new scene if any
                if (_currentScene != null)
                {
                    Components.Add(_currentScene);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // render everything to the target
            GraphicsDevice.SetRenderTarget(_target);

            // draw the scene
            base.Draw(gameTime);

            // done rendering to the target
            GraphicsDevice.SetRenderTarget(null);

            // render the target to the screen
            //_targetBatch.Begin();
            _targetBatch.Begin(samplerState: SamplerState.PointClamp);
            _targetBatch.Draw(_target, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            _targetBatch.End();
        }

        private void LoadScene(Scene scene)
        {
            _nextScene = scene;
        }

        internal void LoadScene(SceneType sceneType, params dynamic[] args)
        {
            switch(sceneType)
            {
                case SceneType.MainMenu:
                    LoadScene(new MainMenuScene(this));
                    break;
                case SceneType.Level:
                    LoadScene(new LevelScene(args[0], args[1], this));
                    break;
                case SceneType.Select:
                    LoadScene(new SelectScene(this));
                    break;
                case SceneType.Editor:
                    LoadScene(new EditorScene(args[0], args[1], this));
                    break;
                case SceneType.Credit:
                    LoadScene(new CreditScene(this));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Tries to load the level with the given index. Returns false if the level does not exist.
        /// </summary>
        /// <param name="levelNumber"></param>
        /// <returns></returns>
        internal bool LoadLevel(string worldName, int levelNumber)
        {
            string path = LevelScene.GetPath(worldName, levelNumber);

            // cannot load if dne
            if(!LevelScene.Exists(path)) return false;

            LoadScene(SceneType.Level, path, false);

            return true;
        }

        internal bool LoadLevel(string path)
        {
            if (!File.Exists(path)) return false;

            LoadScene(SceneType.Level, path, false);

            return true;
        }

        internal void LoadEditor(string worldName, int levelNumber)
        {
            LoadScene(SceneType.Editor, worldName, levelNumber);
        }

        internal void ResetTitle()
        {
            this.SetTitle(Constants.APPLICATION_NAME_UNSAFE);
        }

        internal void SetLevelTitle(string worldName, int levelNumber)
        {
            this.SetTitle($"{Constants.APPLICATION_NAME_UNSAFE}    {worldName}, level {levelNumber}");
        }
    }

    public static class GameExtensions
    {
        public static void SetTitle(this Game game, string title)
        {
            game.Window.Title = title;
        }
    }
}