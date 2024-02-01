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

namespace MiniGolf
{
    internal class EditorScene : Scene
    {
        private const string CUSTOM_NAME = "custom";

        private Texture2D _componentsTexture;
        private LevelInfo _levelInfo;
        private LevelData _levelData;
        private bool _shouldDragSelect = false;
        private bool _universalDrag = false;
        public bool UniversalDrag => _universalDrag;
        private int _selectedTypeIndex = 0;
        private ObjectType SelectedType => (ObjectType)_selectedTypeIndex;

        private bool _movingCamera = false;
        private Vector2 _movingCameraOffset;

        private SpriteObject _preview;
        private SelectionObject _selectionObject;

        private CanvasObject _canvas;

        private readonly Dictionary<ObjectType, List<EditorObject>> _editorObjects = new();

        public EditorScene(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            _canvas = Instantiate(new CanvasObject(this));

            base.Initialize();
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
            //string texturePath = Path.ChangeExtension(customPath, "png");
            string scenePath = Path.ChangeExtension(customPath, "txt");

            // create scene file if necessary
            if (!File.Exists(scenePath)) File.Create(scenePath).Close();

            // create empty preview
            _preview = Instantiate(new SpriteObject(null, new Vector2(100, 100), this)
            {
                Depth = 0.9f
            }, new Vector2(10.0f, Constants.RESOLUTION_HEIGHT - 110.0f), 0.0f, _canvas);

            _componentsTexture = ExternalContent.LoadTexture2D(Path.Combine(path, "components.png"));
            _levelInfo = new LevelInfo(Path.Combine(path, "info.txt"));
            _levelData = new LevelData(scenePath);

            ReloadPreview();

            Load();

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            ButtonState leftControlButtonState = Input.GetKeyboardButtonState(Keys.LeftControl);
            ButtonState leftShiftButtonState = Input.GetKeyboardButtonState(Keys.LeftShift);
            int scroll = Input.GetMouseDeltaScrollY();
            Vector2 mousePosition = Input.MousePosition;

            // exit
            if (Input.GetKeyboardButtonState(Keys.Escape) == ButtonState.Down)
            {
                SaveAndExit();
                return;
            }
            
            // test
            if(Input.GetKeyboardButtonState(Keys.Enter) == ButtonState.Down)
            {
                SaveAndTest();
                return;
            }

            // change selected type
            if (Input.GetKeyboardButtonState(Keys.Up) == ButtonState.Down || (leftControlButtonState <= ButtonState.Down && scroll > 0))
            {
                // move to next object
                _selectedTypeIndex = (_selectedTypeIndex + 1) % ObjectTypeExtensions.OBJECT_TYPE_COUNT;

                ReloadPreview();
            }
            else if (Input.GetKeyboardButtonState(Keys.Down) == ButtonState.Down || (leftControlButtonState <= ButtonState.Down && scroll < 0))
            {
                // move to previous object
                _selectedTypeIndex = (_selectedTypeIndex - 1 + ObjectTypeExtensions.OBJECT_TYPE_COUNT) % ObjectTypeExtensions.OBJECT_TYPE_COUNT;

                ReloadPreview();
            }

            // place new items
            if(Input.GetMouseButtonState(Input.MouseButton.Right) == ButtonState.Down ||
                Input.GetKeyboardButtonState(Keys.Space) == ButtonState.Down)
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

            // drag camera around
            ButtonState middleMouseButtonState = Input.GetMouseButtonState(Input.MouseButton.Middle);
            if (middleMouseButtonState == ButtonState.Down)
            {
                _movingCamera = true;
                _movingCameraOffset = CameraPosition - mousePosition;
            }

            if(_movingCamera)
            {
                CameraPosition = (mousePosition + _movingCameraOffset) / LocalScale;
            }

            // stop dragging camera around
            if(middleMouseButtonState == ButtonState.Up)
            {
                _movingCamera = false;
            }

            // scroll/zoom in and out
            if(leftControlButtonState >= ButtonState.Up)
            {
                if (scroll > 0)
                {
                    // zoom in
                    LocalScale = new Vector2(LocalScale.X * Constants.CAMERA_ZOOM_FACTOR);
                }

                if (scroll < 0)
                {
                    // zoom out
                    LocalScale = new Vector2(LocalScale.X / Constants.CAMERA_ZOOM_FACTOR);
                }
            }

            ButtonState leftMouseButtonState = Input.GetMouseButtonState(Input.MouseButton.Left);

            _shouldDragSelect = leftMouseButtonState == ButtonState.Down;

            base.Update(gameTime);

            // nothing was selected, but the mouse was clicked down
            if (_shouldDragSelect)
            {
                _universalDrag = false;
                _selectionObject = Instantiate(new SelectionObject(mousePosition, this));
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

                            bool selected = false;

                            foreach (Vector2 corner in corners)
                            {
                                if (
                                    corner.X >= selectionHitbox.Position.X &&
                                    corner.X <= selectionHitbox.Position.X + selectionHitbox.Size.X &&
                                    corner.Y >= selectionHitbox.Position.Y &&
                                    corner.Y <= selectionHitbox.Position.Y + selectionHitbox.Size.Y)
                                {
                                    // any corner is inside of this selection box
                                    selected = true;
                                    selectedObjects.Add(editorObject);
                                    break;
                                }
                            }

                            if (!selected)
                            {
                                unselectedObjects.Add(editorObject);
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
                Constants.APPLICATION_NAME,
                CUSTOM_NAME));
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

        #endregion

        #region Objects

        private Sprite GetObjectTypeSprite(ObjectTypeData data)
        {
            return new Sprite(_componentsTexture, data.Rect, data.Pivot);
        }

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
            Sprite sprite = GetObjectTypeSprite(typeData);

            // cannot spawn if null
            if (sprite == null) return null;

            EditorObject editorObject = InstantiateEditorObject(new EditorObject(typeData.Type, sprite, this));

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

        #endregion
    }
}
