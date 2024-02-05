using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MiniGolf
{
    internal abstract class Scene : GameObject
    {
        private readonly ContentManager _contentManager;
        public ContentManager Content => _contentManager;

        private readonly ExternalContentManager _externalContentManager;
        public ExternalContentManager ExternalContent => _externalContentManager;

        private readonly SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch => _spriteBatch;

        private Sprite _backgroundSprite = null;
        public Sprite BackgroundSprite
        {
            get => _backgroundSprite;
            set => SetBackgroundSprite(value);
        }
        private Sprite _foregroundSprite = null;
        public Sprite ForegroundSprite
        {
            get => _foregroundSprite;
            set => SetForegroundSprite(value);
        }
        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;

        protected MiniGolfGame MiniGolfGame => (MiniGolfGame)Game;

        public Vector2 CameraPosition
        {
            get => GetCameraPosition();
            set => SetCameraPosition(value);
        }

        public Vector2 CameraPivot { get; set; } = Vector2.Zero;
        public Vector2 CameraSize => new(Constants.RESOLUTION_WIDTH, Constants.RESOLUTION_HEIGHT);
        public Vector2 CameraOffset => -CameraSize * CameraPivot;
        public Vector2 CameraCenter
        {
            get => GetCameraPosition() - CameraSize * 0.5f;
            set => SetCameraPosition(value + CameraSize * 0.5f);
        }

        public bool Paused { get; set; }

        public Scene(Game game) : base(game)
        {
            // initialize this class
            _contentManager = new ContentManager(game.Services, Constants.CONTENT_ROOT_DIRECTORY);
            _externalContentManager = new ExternalContentManager(game);
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            SetCameraPosition(Vector2.Zero);
        }

        public override void Update(GameTime gameTime)
        {
            if(!Paused)
            {
                base.Update(gameTime);
            }
        }

        protected override void UnloadContent()
        {
            Content.Unload();
            ExternalContent.Unload();

            base.UnloadContent();
        }
        
        private void BeginDraw()
        {
            // draw with layers
            _spriteBatch.Begin(SpriteSortMode.FrontToBack);
        }

        private void EndDraw()
        {
            // done 
            _spriteBatch.End();
        }

        public override void Draw(GameTime gameTime)
        {
            // do nothing if not visible
            if(!Visible) return;

            BeginDraw();

            // draw background first
            GraphicsDevice.Clear(BackgroundColor);
            if(BackgroundSprite != null || ForegroundSprite != null)
            {
                Vector2 position = GetGlobalPosition();
                float rotation = GetGlobalRotation();

                if (BackgroundSprite != null)
                {
                    SpriteBatch.Draw(
                        BackgroundSprite.Texture,
                        new Rectangle((int)MathF.Floor(position.X), (int)MathF.Floor(position.Y), (int)MathF.Floor(BackgroundSprite.Size.X), (int)MathF.Floor(BackgroundSprite.Size.Y)),
                        BackgroundSprite.Slice,
                        Color.White,
                        MathHelper.ToRadians(rotation),
                        BackgroundSprite.Size * BackgroundSprite.Pivot,
                        SpriteEffects.None,
                        0.0f);
                }

                if (ForegroundSprite != null)
                {
                    SpriteBatch.Draw(
                        ForegroundSprite.Texture,
                        new Rectangle((int)MathF.Floor(position.X), (int)MathF.Floor(position.Y), (int)MathF.Floor(ForegroundSprite.Size.X), (int)MathF.Floor(ForegroundSprite.Size.Y)),
                        ForegroundSprite.Slice,
                        Color.White,
                        MathHelper.ToRadians(rotation),
                        ForegroundSprite.Size * ForegroundSprite.Pivot,
                        SpriteEffects.None,
                        0.75f);
                }
            }

            

            // draw other things in the scene
            base.Draw(gameTime);

            EndDraw();
        }

        public T Instantiate<T>(T gameObject) where T : GameObject
        {
            return Instantiate(gameObject, this);
        }

        public T Instantiate<T>(T gameObject, GameObject parent) where T : GameObject
        {
            gameObject.SetParent(parent);
            gameObject.Initialize();
            return gameObject;
        }

        public T Instantiate<T>(T gameObject, Vector2 position, GameObject parent = null) where T : GameObject
        {
            return Instantiate(gameObject, position, 0.0f, parent);
        }

        public T Instantiate<T>(T gameObject, Vector2 position, float rotation, GameObject parent = null) where T : GameObject
        {
            gameObject.SetParent(parent ?? this);
            gameObject.SetOrientation(position, Vector2.One, gameObject.LocalSize, rotation);
            gameObject.Initialize();
            return gameObject;
        }

        public virtual void Clean(GameObject gameObject) { }

        public virtual void Destroy(GameObject gameObject)
        {
            Clean(gameObject);

            gameObject.Destroy();
        }

        private void SetCameraPosition(Vector2 position)
        {
            LocalPosition = position - CameraOffset;
        }

        private Vector2 GetCameraPosition()
        {
            return LocalPosition + CameraOffset;
        }

        private void SetBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
        }

        private void SetForegroundSprite(Sprite sprite)
        {
            _foregroundSprite = sprite;
        }
    }
}
