using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
    internal class EditorScene : Scene
    {
        private const string CUSTOM_NAME = "custom";

        private Texture2D _componentsTexture;
        private LevelInfo _levelInfo;
        private int _objectIndex = 0;

        private SpriteObject _preview;

        public EditorScene(Game game) : base(game)
        {

        }

        protected override void LoadContent()
        {
            // get path to My Games, then make sure it exists
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // make sure the folder for mini golf exists
            path = Path.Combine(path, Constants.APPLICATION_NAME);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string customPath = Path.Combine(path, CUSTOM_NAME);

            // TODO: load texture
            string texturePath = Path.ChangeExtension(customPath, "png");
            string scenePath = Path.ChangeExtension(customPath, "txt");

            // create scene file if necessary
            if(!File.Exists(scenePath)) File.Create(scenePath).Close();

            // create empty preview
            _preview = Instantiate(new SpriteObject(null, new Vector2(100, 100), this)
            {
                Depth = 0.9f
            }, new Vector2(10.0f, 10.0f), 0.0f);

            _componentsTexture = ExternalContent.LoadTexture2D(Path.Combine(path, "components.png"));
            _levelInfo = new LevelInfo(Path.Combine(path, "info.txt"));

            ReloadPreview();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // check for inputs
            if(Input.GetKeyboardButtonState(Keys.Up) == ButtonState.Down)
            {
                // move to next object
                _objectIndex = (_objectIndex + 1) % ObjectTypeExtensions.OBJECT_TYPE_COUNT;

                ReloadPreview();
            }
            else if(Input.GetKeyboardButtonState(Keys.Down) == ButtonState.Down)
            {
                // move to previous object
                _objectIndex = (_objectIndex - 1 + ObjectTypeExtensions.OBJECT_TYPE_COUNT) % ObjectTypeExtensions.OBJECT_TYPE_COUNT;

                ReloadPreview();
            }

            base.Update(gameTime);
        }

        private void ReloadPreview()
        {
            // get the type
            ObjectType type = (ObjectType)_objectIndex;

            // get the type data, create sprite from that if able
            if(_levelInfo.ObjectTypeDatas.TryGetValue(type, out ObjectTypeData value))
            {
                _preview.Sprite = new Sprite(_componentsTexture, value.Rect, Vector2.Zero);
            }
            else
            {
                _preview.Sprite = null;
            }
        }
    }
}
