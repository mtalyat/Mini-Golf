using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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

        private static MiniGolfGame _instance;
        public static MiniGolfGame Instance => _instance;

        public MiniGolfGame()
        {
            _instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = Constants.CONTENT_ROOT_DIRECTORY;
            IsMouseVisible = true;
        }

        ~MiniGolfGame()
        {
            _instance = null;
        }

        protected override void Initialize()
        {
            // initialize the window
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowAltF4 = true;

            // create target so the game can scale with the window
            _targetBatch = new SpriteBatch(GraphicsDevice);
            _target = new RenderTarget2D(GraphicsDevice, Constants.RESOLUTION_WIDTH, Constants.RESOLUTION_HEIGHT);

            // create the level, the scene does the rest
            LoadScene(new MainMenuScene(this));

            base.Initialize();
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            
        }

        protected override void LoadContent()
        {
            

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == XnaButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            Input.Update(this);

            if(_nextScene != null)
            {
                // unload current scene if any
                if (_currentScene != null)
                {
                    Components.Remove(_currentScene);
                    _currentScene.Destroy();
                }

                _currentScene = _nextScene;
                _nextScene = null;

                // load new scene if any
                if (_currentScene != null)
                {
                    Components.Add(_currentScene);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // render everything to the target
            GraphicsDevice.SetRenderTarget(_target);

            // use the scene background color to clear
            GraphicsDevice.Clear(_currentScene.BackgroundColor);

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

        internal void LoadScene(SceneType sceneType, int data = 0)
        {
            switch(sceneType)
            {
                case SceneType.MainMenu:
                    LoadScene(new MainMenuScene(this));
                    break;
                case SceneType.Level:
                    LoadScene(new LevelScene(data, this));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}