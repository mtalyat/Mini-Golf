using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace MiniGolf
{
    internal class ExternalContentManager
    {
        private readonly Game _game;

        private readonly Dictionary<string, object> _content = new();
        private readonly List<IDisposable> _disposableContent = new();

        public ExternalContentManager(Game game)
        {
            _game = game;
        }

        public string[] ReadText(string path)
        {
            return File.ReadAllLines(path);
        }

        public Texture2D LoadTexture2D(string path)
        {
            // check if already contains, return if it does
            if (_content.TryGetValue(path, out object value))
            {
                return (Texture2D)value;
            }

            // if file does not exist, return null
            if(!File.Exists(path))
            {
                return null;
            }

            // load texture from file
            Texture2D texture = Texture2D.FromFile(_game.GraphicsDevice, path);

            // convert to proper color values
            // https://stackoverflow.com/questions/40194266/how-to-load-transparent-png-using-texture2d-fromstream
            Color[] buffer = new Color[texture.Width * texture.Height];
            texture.GetData(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                Color color = buffer[i];
                buffer[i] = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
            }
            texture.SetData(buffer);

            //Texture2D texture = Texture2D.FromFile(_game.GraphicsDevice, path);

            // save so we don't have to reload it
            _content[path] = texture;
            _disposableContent.Add(texture);

            // return reference
            return texture;
        }

        public void Unload()
        {
            // unload all
            foreach (IDisposable disposable in _disposableContent)
            {
                disposable?.Dispose();
            }

            _content.Clear();
            _disposableContent.Clear();
        }

        public void UnloadAsset(string path)
        {
            // unload given asset
            if (_content.TryGetValue(path, out object value))
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                    _disposableContent.Remove(disposable);
                }
                _content.Remove(path);
            }
        }
    }
}
