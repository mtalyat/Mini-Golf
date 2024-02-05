using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;

namespace MiniGolf
{
    internal class EditorScene : NavigatableScene
    {
        private readonly string _worldName;
        private readonly int _levelNumber;

        private Texture2D _componentsTexture;
        private LevelInfo _levelInfo;
        private LevelData _levelData;
        private bool _shouldDragSelect = false;
        private bool _universalDrag = false;
        public bool UniversalDrag => _universalDrag;
        private int _selectedTypeIndex = (int)ObjectType.Hole;
        private ObjectType SelectedType => (ObjectType)_selectedTypeIndex;

        private SpriteObject _preview;
        private SelectionObject _selectionObject;

        private readonly List<BallType> _balls = new();
        private readonly List<SpriteObject> _ballPreviews = new();
        private SpriteObject _plus;

        private CanvasObject _canvas;
        private SpriteObject _pauseMenu;
        private SoundEffect _pauseSfx;

        private readonly Dictionary<ObjectType, List<EditorObject>> _editorObjects = new();

        public EditorScene(string worldName, int levelNumber, Game game) : base(game)
        {
            _worldName = worldName;
            _levelNumber = levelNumber;
        }

        public override void Initialize()
        {
            _canvas = Instantiate(new CanvasObject(this));

            MiniGolfGame.SetLevelTitle(_worldName, _levelNumber);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // get path to My Games, then make sure it exists
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // make sure the folder for mini golf exists
            path = Path.Combine(path, Constants.APPLICATION_NAME_SAFE);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // make sure the custom world folder exists
            path = Path.Combine(path, _worldName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string levelPath = Path.Combine(path, $"level{_levelNumber}");
            string scenePath = Path.ChangeExtension(levelPath, Constants.PATH_LEVEL_EXTENSION);

            // create scene file if necessary
            if (!File.Exists(scenePath)) File.Create(scenePath).Close();

            // create empty preview
            _preview = Instantiate(new SpriteObject(null, new Vector2(100, 100), this)
            {
                Depth = 0.9f
            }, new Vector2(10.0f, Constants.RESOLUTION_HEIGHT - 110.0f), 0.0f, _canvas);

            _componentsTexture = ExternalContent.LoadTexture2D(Path.Combine(path, "components.png"));
            _levelInfo = new LevelInfo(Path.Combine(path, $"info{Constants.PATH_INFO_EXTENSION}"));
            _levelData = new LevelData(scenePath);

            // load the level pngs
            Texture2D backgroundTexture = ExternalContent.LoadTexture2D(Path.ChangeExtension($"{Path.ChangeExtension(levelPath, null)}bg", "png"));
            if (backgroundTexture != null)
            {
                BackgroundSprite = new Sprite(backgroundTexture);
                LocalSize = BackgroundSprite.Size;
            }
            Texture2D foregroundTexture = ExternalContent.LoadTexture2D(Path.ChangeExtension($"{Path.ChangeExtension(levelPath, null)}fg", "png"));
            if (foregroundTexture != null)
            {
                ForegroundSprite = new Sprite(foregroundTexture);
            }
            BackgroundColor = _levelData.TakeColor(new Color(37, 121, 9));

            ReloadPreview();

            _plus = Instantiate(new SpriteObject(new Sprite(Content.Load<Texture2D>("Texture/Plus"), null, new Vector2(0.5f)), this)
            {
                Depth = 0.9f
            }, _canvas);
            _balls.AddRange(_levelData.TakeBalls());
            ReloadBallPreviews();

            Load();
            LoadPauseMenu();            

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if(Input.GetKeyboardButtonState(Keys.Escape) == ButtonState.Down)
            {
                SetPause(!Paused);
            }

            if(Paused)
            {
                // only update pause menu
                _pauseMenu.Update(gameTime);
                return;
            }

            ButtonState leftControlButtonState = Input.GetKeyboardButtonState(Keys.LeftControl);
            ButtonState leftShiftButtonState = Input.GetKeyboardButtonState(Keys.LeftShift);
            int scroll = Input.GetMouseDeltaScrollY();
            //Vector2 mousePosition = Input.MousePosition;
            Vector2 globalMousePosition = Input.GetMouseGlobalPosition(this);

            // go to next level
            if(Input.GetKeyboardButtonState(Keys.Up) == ButtonState.Down)
            {
                Save();
                LoadNextLevel(1);
                return;
            }

            // go to prev level
            if(Input.GetKeyboardButtonState(Keys.Down) == ButtonState.Down)
            {
                Save();
                LoadNextLevel(-1);
                return;
            }

            // skip 10 levels
            if (Input.GetKeyboardButtonState(Keys.PageUp) == ButtonState.Down)
            {
                Save();
                LoadNextLevel(10);
                return;
            }
            
            // go back 10 levels
            if (Input.GetKeyboardButtonState(Keys.PageDown) == ButtonState.Down)
            {
                Save();
                LoadNextLevel(-10);
                return;
            }

            // change selected type
            if (Input.GetKeyboardButtonState(Keys.R) == ButtonState.Down || (leftControlButtonState <= ButtonState.Down && scroll > 0))
            {
                // move to next object
                _selectedTypeIndex = (_selectedTypeIndex - 1) % (ObjectTypeExtensions.OBJECT_TYPE_COUNT - 2) + 2;

                ReloadPreview();
            }
            else if (Input.GetKeyboardButtonState(Keys.F) == ButtonState.Down || (leftControlButtonState <= ButtonState.Down && scroll < 0))
            {
                // move to previous object
                _selectedTypeIndex = (_selectedTypeIndex - 3 + (ObjectTypeExtensions.OBJECT_TYPE_COUNT - 2)) % (ObjectTypeExtensions.OBJECT_TYPE_COUNT - 2) + 2;

                ReloadPreview();
            }

            // place new items
            if(Input.GetKeyboardButtonState(Keys.Space) == ButtonState.Down)
            {
                if(leftControlButtonState <= ButtonState.Down)
                {
                    // duplicate selected if any
                    // unselect old, select new
                    foreach(var pair in _editorObjects)
                    {
                        // iterate backwards in case a new object is added to this list
                        for(int i = pair.Value.Count - 1; i >= 0; i--)
                        {
                            EditorObject obj = pair.Value[i];
                            if (!obj.Selected) continue;
                            obj.Selected = false;

                            EditorObject newObj = SpawnTypeObject(_levelInfo.ObjectTypeDatas[obj.Type]);
                            newObj.SetOrientation(obj);
                            newObj.LocalPosition += new Vector2(20.0f, -20.0f); // offset a little so its not stuck behind it
                            newObj.Selected = true;
                        }
                    }
                }
                else
                {
                    // spawn new object
                    if(leftShiftButtonState >= ButtonState.Up)
                    {
                        SetAllSelected(false);
                    }
                    EditorObject obj = SpawnSelectedTypeObject();
                    if(obj != null)
                    {
                        obj.Selected = true;
                    }
                }
            }

            UpdateBallPreviews(gameTime);

            ButtonState leftMouseButtonState = Input.GetMouseButtonState(Input.MouseButton.Left);

            _shouldDragSelect = leftMouseButtonState == ButtonState.Down;

            base.Update(gameTime);

            // nothing was selected, but the mouse was clicked down
            if (_shouldDragSelect)
            {
                _universalDrag = false;
                _selectionObject = Instantiate(new SelectionObject(globalMousePosition, this));
            }
            else if (leftMouseButtonState == ButtonState.Down)
            {
                // something was selected
                _universalDrag = true;
            }

            // if released mouse, unselect
            if (leftMouseButtonState == ButtonState.Up)
            {
                _universalDrag = false;

                if (_selectionObject != null)
                {
                    // check each object: if all 4 corners are within the selection, we are good
                    Hitbox selectionHitbox = _selectionObject.GetHitbox();

                    List<EditorObject> selectedObjects = new();
                    List<EditorObject> unselectedObjects = new();

                    foreach (var pair in _editorObjects)
                    {
                        foreach (EditorObject editorObject in pair.Value)
                        {
                            Vector2[] corners = editorObject.GetHitbox().GetCorners();

                            bool selected = true;

                            foreach (Vector2 corner in corners)
                            {
                                if (
                                    corner.X < selectionHitbox.Position.X ||
                                    corner.X > selectionHitbox.Position.X + selectionHitbox.Size.X ||
                                    corner.Y < selectionHitbox.Position.Y ||
                                    corner.Y > selectionHitbox.Position.Y + selectionHitbox.Size.Y)
                                {
                                    // any corner is inside of this selection box
                                    selected = false;
                                    unselectedObjects.Add(editorObject);
                                    break;
                                }
                            }

                            if (selected)
                            {
                                selectedObjects.Add(editorObject);
                            }
                        }
                    }

                    if (leftControlButtonState <= ButtonState.Down)
                    {
                        // select the selected list only
                        foreach (var obj in selectedObjects)
                        {
                            obj.Selected = !obj.Selected;
                        }
                    }
                    else
                    {
                        // select the selected list only
                        foreach (var obj in selectedObjects)
                        {
                            obj.Selected = true;
                        }

                        // if holding left shift, do not unselected non-included objects
                        if (leftShiftButtonState >= ButtonState.Up)
                        {
                            foreach (var obj in unselectedObjects)
                            {
                                obj.Selected = false;
                            }
                        }
                    }

                    // clean up
                    _selectionObject.Destroy();
                    _selectionObject = null;
                }
            }
        }

        public override void Clean(GameObject gameObject)
        {
            if(gameObject is EditorObject editorObject)
            {
                _editorObjects[editorObject.Type].Remove(editorObject);
            }

            base.Clean(gameObject);
        }

        #region Saving and Loading

        private void Save()
        {
            // compile values
            _levelData.AddValue("Balls", _balls);
            _levelData.AddValue("Color", $"{BackgroundColor.R} {BackgroundColor.G} {BackgroundColor.B} {BackgroundColor.A}");

            // compile level data
            _levelData.ObjectDatas.Clear();

            foreach(var pair in _editorObjects)
            {
                List<ObjectData> datas = new();
                _levelData.ObjectDatas.Add(pair.Key, datas);

                foreach(EditorObject editorObject in pair.Value)
                {
                    datas.Add(editorObject.ToObjectData());
                }
            }

            // save
            _levelData.Save();
        }

        private void Load()
        {
            // destroy existing
            foreach(var pair in _editorObjects)
            {
                for(int i = pair.Value.Count - 1;  i >= 0; i--)
                {
                    Destroy(pair.Value[i]);
                }
            }
            _editorObjects.Clear();

            // load new ones
            foreach(var pair in _levelData.ObjectDatas)
            {
                if(_levelInfo.ObjectTypeDatas.TryGetValue(pair.Key, out var typeData))
                {
                    foreach(ObjectData data in pair.Value)
                    {
                        SpawnTypeObject(typeData, data);
                    }
                }
            }
        }

        private void Test()
        {
            ((MiniGolfGame)Game).LoadScene(SceneType.Level, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "My Games",
                Constants.APPLICATION_NAME_SAFE,
                _worldName,
                $"level{_levelNumber}.txt"), true);
        }

        private void Exit()
        {
            ((MiniGolfGame)Game).LoadScene(SceneType.MainMenu);
        }

        private void SaveAndExit()
        {
            Save();
            Exit();
        }

        private void SaveAndTest()
        {
            Save();
            Test();
        }

        private void LoadNextLevel(int direction)
        {
            MiniGolfGame game = (MiniGolfGame)Game;

            game.LoadEditor(_worldName, _levelNumber + direction);
        }

        #endregion

        #region Pause

        private void SetPause(bool pause)
        {
            if (pause != Paused)
            {
                _pauseSfx.Play();
                Paused = pause;
                _pauseMenu.Visible = pause;
            }
        }

        private void LoadPauseMenu()
        {
            _pauseSfx = Content.Load<SoundEffect>("Audio/Woosh");

            Texture2D uiTexture = Content.Load<Texture2D>("Texture/UI");

            const float pauseMenuSpacing = 20.0f;
            const float pauseMenuSpacing2 = pauseMenuSpacing * 2.0f;
            const float pauseMenuDepth = 0.95f;
            const float pauseMenuItemDepth = pauseMenuDepth + 0.001f;

            _pauseMenu = Instantiate(new SpriteObject(new Sprite(uiTexture, Constants.UI_BACKGROUND), new Vector2(320.0f, Constants.RESOLUTION_HEIGHT), pauseMenuDepth, this), _canvas);
            float pauseMenuWidth = _pauseMenu.LocalSize.X - pauseMenuSpacing2;
            Instantiate(new TextObject("Pause", new Vector2(pauseMenuWidth, 100.0f), pauseMenuItemDepth, this), new Vector2(pauseMenuSpacing, pauseMenuSpacing), _pauseMenu);
            Instantiate(new ButtonObject("Resume", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                SetPause(false);
            })
            {
                Depth = pauseMenuItemDepth,
                LocalSize = new Vector2(pauseMenuWidth, 80.0f),
                Margin = 0.0625f,
            }, new Vector2(pauseMenuSpacing, pauseMenuSpacing * 2.0f + 100.0f), _pauseMenu);
            Instantiate(new ButtonObject("Save", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                Save();
            })
            {
                Depth = pauseMenuItemDepth,
                LocalSize = new Vector2(pauseMenuWidth, 80.0f),
                Margin = 0.0625f,
            }, new Vector2(pauseMenuSpacing, pauseMenuSpacing * 3.0f + 200.0f), _pauseMenu);
            Instantiate(new ButtonObject("Save and Test", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                SaveAndTest();
            })
            {
                Depth = pauseMenuItemDepth,
                LocalSize = new Vector2(pauseMenuWidth, 80.0f),
                Margin = 0.0625f,
            }, new Vector2(pauseMenuSpacing, pauseMenuSpacing * 4.0f + 300.0f), _pauseMenu);
            Instantiate(new ButtonObject("Save and Exit", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                SaveAndExit();
            })
            {
                Depth = pauseMenuItemDepth,
                LocalSize = new Vector2(pauseMenuWidth, 80.0f),
                Margin = 0.0625f,
            }, new Vector2(pauseMenuSpacing, pauseMenuSpacing * 5.0f + 400.0f), _pauseMenu);

            _pauseMenu.Visible = false;
        }

        #endregion

        #region Objects

        // spawns the selected type
        private EditorObject SpawnSelectedTypeObject()
        {
            if(_levelInfo.ObjectTypeDatas.TryGetValue(SelectedType, out ObjectTypeData data))
            {
                EditorObject editorObject = SpawnTypeObject(data);

                // move to mouse position
                editorObject.LocalPosition = Input.GetMouseGlobalPosition(this);

                return editorObject;
            }

            return null;
        }

        private EditorObject SpawnTypeObject(ObjectTypeData typeData, ObjectData data = null)
        {
            EditorObject editorObject = InstantiateEditorObject(new EditorObject(typeData, _componentsTexture, this));

            if(data != null)
            {
                editorObject.SetOrientation(data.Position, Vector2.One, data.Size, data.Rotation);
            }

            return editorObject;
        }

        private EditorObject InstantiateEditorObject(EditorObject editorObject)
        {
            // spawn in center by default
            Instantiate(editorObject, new Vector2(Constants.RESOLUTION_WIDTH / 2.0f, Constants.RESOLUTION_HEIGHT / 2.0f), 0.0f);

            // create list for type if it does not exist
            if(!_editorObjects.TryGetValue(editorObject.Type, out List<EditorObject> editorObjects))
            {
                editorObjects = new List<EditorObject>();
                _editorObjects.Add(editorObject.Type, editorObjects);
            }

            // add to that list
            editorObjects.Add(editorObject);

            return editorObject;
        }

        #endregion

        #region Selecting

        public void DoNotDragSelect()
        {
            _shouldDragSelect = false;
        }

        public void SetAllSelected(bool selected)
        {
            foreach(var pair in _editorObjects)
            {
                foreach(EditorObject editorObject in pair.Value)
                {
                    editorObject.Selected = selected;
                }
            }
        }

        #endregion

        #region UI

        private void ReloadPreview()
        {
            // get the type
            ObjectType type = (ObjectType)_selectedTypeIndex;

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

        private const float PREVIEW_OFFSET = 45.0f;
        private const float PREVIEW_SIZE = 50.0f;
        private const float PREVIEW_SPACING = 70.0f;

        private Sprite CreateBallPreviewSprite(BallType type)
        {
            return new Sprite(Content.Load<Texture2D>($"Texture/{type}"), null, new Vector2(0.5f, 0.5f));
        }

        private void UpdateBallPreviews(GameTime gameTime)
        {
            for (int i = _ballPreviews.Count - 1; i >= 0; i--)
            {
                SpriteObject spriteObject = _ballPreviews[i];

                if (Input.ContainsMouse(spriteObject.GetHitbox()))
                {
                    // if left, add
                    if (Input.GetMouseButtonState(Input.MouseButton.Left) == ButtonState.Down)
                    {
                        _balls[i] = (BallType)(((int)_balls[i] + 1) % BallTypeExtensions.BALL_TYPE_COUNT);
                        spriteObject.Sprite = CreateBallPreviewSprite(_balls[i]);
                    }

                    // if right, subtract
                    if (Input.GetMouseButtonState(Input.MouseButton.Right) == ButtonState.Down)
                    {
                        _balls[i] = (BallType)(((int)_balls[i] - 1 + BallTypeExtensions.BALL_TYPE_COUNT) % BallTypeExtensions.BALL_TYPE_COUNT);
                        spriteObject.Sprite = CreateBallPreviewSprite(_balls[i]);
                    }

                    // if middle click, delete
                    if (Input.GetMouseButtonState(Input.MouseButton.Middle) == ButtonState.Down)
                    {
                        _balls.RemoveAt(i);
                        ReloadBallPreviews();

                        // mouse can only be over one at a time, so stop iterating
                        return;
                    }

                    // spin when mouse is hovering and not clicking
                    const float SPIN_SPEED = -60.0f; // degree/s
                    spriteObject.LocalRotation += SPIN_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // mouse can only be over one at a time, so stop iterating
                    return;
                }
                else
                {
                    spriteObject.LocalRotation = 0.0f;
                }
            }

            // if plus clicked, add to it
            if(Input.ContainsMouse(_plus) && Input.GetMouseButtonState(Input.MouseButton.Left) == ButtonState.Down)
            {
                _balls.Add(BallType.GolfBall);
                ReloadBallPreviews();
                return;
            }
        }

        private Vector2 GetBallPreviewPosition(int i)
        {
            return new Vector2(PREVIEW_OFFSET + PREVIEW_SPACING * i, PREVIEW_OFFSET);
        }

        private void ReloadBallPreviews()
        {
            // destroy old balls
            for(int i = _ballPreviews.Count - 1; i >= 0; i--)
            {
                _ballPreviews[i].Destroy();
            }
            _ballPreviews.Clear();

            // create new balls
            for (int i = 0; i < _balls.Count; i++)
            {
                SpriteObject preview = Instantiate(new SpriteObject(CreateBallPreviewSprite(_balls[i]), this)
                {
                    Depth = 0.9f,
                    LocalPosition = GetBallPreviewPosition(i),
                    LocalSize = new Vector2(PREVIEW_SIZE),
                }, _canvas);
                _ballPreviews.Add(preview);
            }

            // place the plus on the right
            _plus.SetOrientation(GetBallPreviewPosition(_balls.Count), Vector2.One, new Vector2(PREVIEW_SIZE), 0.0f);
        }

        #endregion
    }
}
