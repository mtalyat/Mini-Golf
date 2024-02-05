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
    internal class SelectScene : Scene
    {
        private enum Location
        {
            Builtin,
            Custom
        }

        private LayoutObject _grid;
        private ButtonObject _gridBackButton;

        private int _depth = 0;
        private Location _location;
        private string _world;

        public SelectScene(Game game) : base(game)
        {
        }

        protected override void LoadContent()
        {
            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            _grid = Instantiate(new LayoutObject(new Vector2(500, 500), this)
            {
                CellCount = new Point(0, 0),
                CellOrientation = LayoutObject.Orientation.Grid,
                CellSize = new Vector2(0.0f, 0.0f)
            });

            _gridBackButton = Instantiate(new ButtonObject("Back", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160)), this, (GameObject _) =>
            {
                _depth--;
                Refresh();
            })
            {
                Margin = 20
            }, new Vector2(0, 500));

            Refresh();

            base.LoadContent();
        }

        private void Refresh()
        {
            // reload the grid
            switch (_depth)
            {
                case 0:
                    LoadHeaders();
                    break;
                case 1:
                    LoadLocation();
                    break;
                case 2:
                    LoadWorld();
                    break;
            }

            // hide back button if cannot go back
            _gridBackButton.Visible = _depth > 0;
        }

        private void UnloadGrid()
        {
            _grid.DestroyChildren();
        }

        private void LoadHeaders()
        {
            UnloadGrid();

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            Instantiate(new ButtonObject("Built In", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0, 0)), 20.0f, this, (GameObject _) =>
            {
                _depth++;
                _location = Location.Builtin;
                Refresh();
            })
            {
                Margin = 20
            }, _grid);

            Instantiate(new ButtonObject("Custom", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0, 0)), 20.0f, this, (GameObject _) =>
            {
                _depth++;
                _location = Location.Custom;
                Refresh();
            })
            {
                Margin = 20
            }, _grid);

            _grid.Refresh();
        }

        private void LoadLocation()
        {
            UnloadGrid();

            string directoryPath = _location switch
            {
                Location.Builtin => Constants.PATH_BUILTIN,
                Location.Custom => Constants.PATH_CUSTOM,
                _ => throw new NotImplementedException(),
            };

            // get the world options
            string[] paths = Directory.GetDirectories(directoryPath);

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");
            foreach (string path in paths)
            {
                Instantiate(new ButtonObject(Path.GetFileNameWithoutExtension(path), new Sprite(uiTexture, new Rectangle(0, 0, 320, 160)), this, (GameObject sender) =>
                {
                    ButtonObject button = (ButtonObject)sender;

                    _depth++;
                    _world = button.Text;
                    Refresh();
                })
                {
                    Margin = 20
                }, _grid);
            }

            _grid.Refresh();
        }

        private void LoadWorld()
        {
            UnloadGrid();

            string directoryPath = _location switch
            {
                Location.Builtin => Constants.PATH_BUILTIN,
                Location.Custom => Constants.PATH_CUSTOM,
                _ => throw new NotImplementedException(),
            };

            directoryPath = Path.Combine(directoryPath, _world);

            // get the world level options
            string[] paths = Directory.GetFiles(directoryPath);

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");
            int levelNumber = 1;
            foreach (string path in paths)
            {
                // skip non-level levels
                if (Path.GetExtension(path) != ".level") continue;

                Instantiate(new ButtonObject(levelNumber.ToString(), new Sprite(uiTexture, new Rectangle(0, 0, 320, 160)), this, (GameObject sender) =>
                {
                    ButtonObject button = (ButtonObject)sender;

                    LoadLevel(int.Parse(button.Text));
                })
                {
                    Margin = 20
                }, _grid);

                levelNumber++;
            }

            _grid.Refresh();
        }

        private void LoadLevel(int levelNumber)
        {
            string directoryPath = _location switch
            {
                Location.Builtin => Constants.PATH_BUILTIN,
                Location.Custom => Constants.PATH_CUSTOM,
                _ => throw new NotImplementedException(),
            };

            MiniGolfGame.LoadLevel(Path.Combine(directoryPath, _world, $"level{levelNumber}{Constants.PATH_LEVEL_EXTENSION}"));
        }
    }
}
