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

        private readonly SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch => _spriteBatch;

        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;

        public Scene(Game game) : base(game)
        {
            // initialize this class
            _contentManager = new ContentManager(game.Content.ServiceProvider)
            {
                RootDirectory = Constants.CONTENT_ROOT_DIRECTORY
            };
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            // set size to 1 for normal rendering
            LocalSize = new Vector2(1.0f, 1.0f);
        }

        protected override void UnloadContent()
        {
            Content.Unload();

            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            // draw with layers
            SpriteBatch.Begin(SpriteSortMode.FrontToBack);

            // draw other things in the scene
            base.Draw(gameTime);

            // done drawing
            SpriteBatch.End();
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

        public T Instantiate<T>(T gameObject, Vector2 position, float rotation) where T : GameObject
        {
            return Instantiate(gameObject, position, rotation, this);
        }

        public T Instantiate<T>(T gameObject, Vector2 position, float rotation, GameObject parent) where T : GameObject
        {
            gameObject.SetParent(parent);
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
    }
}
