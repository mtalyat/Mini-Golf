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
        private TextObject _text;

        private int _depth = 0;
        private Location _location;
        private string _world;

        public SelectScene(Game game) : base(game)
        { }

        protected override void LoadContent()
        {
            BackgroundSprite = new Sprite(Content.Load<Texture2D>("Texture/MenuBackground"));

            _text = Instantiate(new TextObject("", new Vector2(Constants.RESOLUTION_WIDTH - 200.0f, 100.0f), 0.9f, this), new Vector2(100.0f, 0.0f));

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            _grid = Instantiate(new LayoutObject(new Vector2(Constants.RESOLUTION_WIDTH - 200, Constants.RESOLUTION_HEIGHT - 200), this)
            {
                CellCount = new Point(0, 0),
                CellOrientation = LayoutObject.Orientation.Grid,
                CellSize = new Vector2(0.0f, 0.0f)
            }, new Vector2(100.0f));

            Instantiate(new ButtonObject("Back", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                if(_depth == 0)
                {
                    // go back to main menu
                    Exit();
                }
                else
                {
                    // go back to prev step
                    _depth--;
                    Refresh();
                }
            })
            {
                Margin = 0.0625f,
                LocalSize = new Vector2(120.0f, 80.0f)
            }, new Vector2(10.0f, Constants.RESOLUTION_HEIGHT - 90.0f));

            Refresh();

            base.LoadContent();
        }

        private void Exit()
        {
            MiniGolfGame.LoadScene(SceneType.MainMenu);
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
        }

        private void UnloadGrid()
        {
            _grid.DestroyChildren();
            _grid.CellCount = Point.Zero;
            _grid.CellSize = Vector2.Zero;
            _grid.CellOrientation = LayoutObject.Orientation.Grid;
        }

        private void LoadHeaders()
        {
            UnloadGrid();

            _text.Content = "Select location:";

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            Instantiate(new ButtonObject("Built In", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0, 0)), 20.0f, this, (GameObject _) =>
            {
                _depth++;
                _location = Location.Builtin;
                Refresh();
            })
            {
                Margin = 0.0625f,
            }, _grid);

            Instantiate(new ButtonObject("Custom", new Sprite(uiTexture, new Rectangle(0, 0, 320, 160), new Vector2(0, 0)), 20.0f, this, (GameObject _) =>
            {
                _depth++;
                _location = Location.Custom;
                Refresh();
            })
            {
                Margin = 0.0625f,
            }, _grid);

            _grid.CellSize = new Vector2(320.0f, 160.0f);
            _grid.Refresh();
        }

        private void LoadLocation()
        {
            UnloadGrid();

            _text.Content = "Select world:";

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
                    Margin = 0.0625f,
                }, _grid);
            }

            _grid.CellSize = new Vector2(320.0f, 160.0f);
            _grid.Refresh();
        }

        private void LoadWorld()
        {
            UnloadGrid();

            _text.Content = "Select level:";

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
                    Margin = 0.0625f,
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
