using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Path = System.IO.Path;

namespace MiniGolf
{
    /// <summary>
    /// The scene for running a level.
    /// </summary>
    internal class LevelScene : NavigatableScene
    {
        private enum State
        {
            Play,
            GameOver
        }

        private readonly string _path;
        private readonly string _worldName;
        private readonly int _levelNumber;
        private readonly SceneType _exitScene;
        private LevelData _data;
        private LevelInfo _info;

        private readonly List<BallType> _balls = new();
        private readonly List<SpriteObject> _ballPreviews = new();

        private readonly Dictionary<ObjectType, List<GameObject>> _typeObjects = new();

        private readonly List<LevelObject> _collisionObjects = new();

        // player management data:
        // holds references to player objects
        private List<Player> _alivePlayers;
        // holds position and rotation
        private List<BallData> _alivePlayerSpawns;
        // current player index in _alivePlayers
        private int _activePlayerIndex = -1;
        // active player with the active ball
        private Player ActivePlayer => _alivePlayers[_activePlayerIndex];

        private readonly int _playerCount = 0;
        private int _winCount = 0;
        private int _loseCount = 0;

        private BallObject _activeBall = null;
        private TextObject _strokeText = null;

        private readonly CanvasObject _canvas;
        private SpriteObject _pauseMenu;
        private SoundEffect _pauseSfx;

        private bool _isFollowingBall = false;

        protected override ButtonState CameraMoveButtonState
        {
            get => base.CameraMoveButtonState.Combine(Input.GetMouseButtonState(Input.MouseButton.Right));
        }

        private State _state;
        private float _timer;

        /// <summary>
        /// Runs a level at the given path.
        /// </summary>
        /// <param name="path">The path to the level.png in the Content folder.</param>
        /// <param name="exitScene">The scene to go to on Exit().</param>
        public LevelScene(string path, SceneType exitScene, Game game) : base(game)
        {
            _path = Path.ChangeExtension(path, null);
            _exitScene = exitScene;
            _alivePlayers = new List<Player>(Session.Players);
            _playerCount = _alivePlayers.Count;
            _canvas = new CanvasObject(this);

            // set defaults
            _levelNumber = 1;
            _worldName = string.Empty;
            foreach(Player player in _alivePlayers)
            {
                player.Reset();
            }

            if (!string.IsNullOrEmpty(_path))
            {
                // parse world name and level number from path if possible
                if (char.IsDigit(_path[^1]))
                {
                    // extract the number from the end of the path
                    // https://stackoverflow.com/questions/13169393/extract-number-at-end-of-string-in-c-sharp
                    var result = Regex.Match(_path, @"\d+$", RegexOptions.RightToLeft);
                    _levelNumber = int.Parse(result.Value);
                }
                _worldName = Directory.GetParent(_path).Name;
            }
        }

        public static string GetPath(string worldName, int levelNumber)
        {
            return Path.GetFullPath(Path.Combine(Constants.PATH_BUILTIN, worldName, $"level{levelNumber}{Constants.PATH_LEVEL_EXTENSION}"));
        }

        public static bool Exists(string worldName, int levelNumber)
        {
            return Exists(GetPath(worldName, levelNumber));
        }

        public static bool Exists(string path)
        {
            return File.Exists(path);
        }

        public override void Initialize()
        {
            Instantiate(_canvas);

            MiniGolfGame.SetLevelTitle(_worldName, _levelNumber);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // create the pause menu
            LoadPauseMenu();

            string fullPath = _path;
            string folderPath = Path.GetDirectoryName(_path);
            string fullFolderPath = Path.GetDirectoryName(fullPath);

            // load level data from the file
            _info = new LevelInfo(Path.Combine(fullFolderPath, $"info{Constants.PATH_INFO_EXTENSION}"));
            _data = new LevelData(Path.ChangeExtension(fullPath, Constants.PATH_LEVEL_EXTENSION));
            _balls.AddRange(_data.TakeBalls());
            // if no balls loaded, add one golf ball
            if (!_balls.Any())
            {
                _balls.Add(BallType.GolfBall);
            }

            // load the level png
            Texture2D backgroundTexture = ExternalContent.LoadTexture2D(Path.ChangeExtension($"{Path.ChangeExtension(fullPath, null)}bg", "png"));
            if(backgroundTexture != null)
            {
                BackgroundSprite = new Sprite(backgroundTexture);
                LocalSize = BackgroundSprite.Size;
            }
            Texture2D foregroundTexture = ExternalContent.LoadTexture2D(Path.ChangeExtension($"{Path.ChangeExtension(fullPath, null)}fg", "png"));
            if (foregroundTexture != null)
            {
                ForegroundSprite = new Sprite(foregroundTexture);
            }
            BackgroundColor = _data.TakeColor();

            // load the level components png
            Texture2D levelComponentsTexture = ExternalContent.LoadTexture2D(Path.Combine(folderPath, "components.png"));

            // create each object
            foreach (var pair in _data.ObjectDatas)
            {
                // add a new list for the type
                _typeObjects.Add(pair.Key, new List<GameObject>());

                foreach (var data in pair.Value)
                {
                    // create object using type and other data
                    InstantiateLevelObject(CreateLevelObject(_info.ObjectTypeDatas[pair.Key], data, levelComponentsTexture));
                }
            }

            // ensure ball is in there
            _typeObjects.TryAdd(ObjectType.Ball, new List<GameObject>());

            // find the starting spawn location
            Vector2 start = Vector2.Zero;
            if(_typeObjects.TryGetValue(ObjectType.Start, out List<GameObject> value))
            {
                start = value[0].LocalCenter;
            }

            // populate the respawn points
            _alivePlayerSpawns = new List<BallData>(Enumerable.Repeat(new BallData(start), _alivePlayers.Count));

            const float strokeTextWidth = 200.0f;
            _strokeText = Instantiate(new TextObject("Stokes: 0", new Vector2(strokeTextWidth, 80.0f), 0.93f, this), new Vector2(Constants.RESOLUTION_WIDTH - strokeTextWidth, 0.0f), _canvas);

            // start the game
            NextTurn();
            SnapCameraToBall();
            LoadInstructions();

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // if press escape, pause
            if(Input.GetKeyboardButtonState(Keys.Escape) == ButtonState.Down)
            {
                SetPause(!Paused);
            }

            if (Paused)
            {
                // update the pause menu only
                _pauseMenu.Update(gameTime);

                return;
            }

            // if press space, follow ball
            if(Input.GetKeyboardButtonState(Keys.Space) == ButtonState.Down)
            {
                SetBallFollow(true);
            }

            // move timer if waiting to move to another scene
            if (_state != State.Play)
            {
                _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_timer <= 0.0f)
                {
                    switch (_state)
                    {
                        case State.GameOver:
                            GameOver();
                            return;
                    }
                }
            }

            // if the ball is in the scene and moving, simulate physics on it
            if (_activeBall != null && _activeBall.IsMoving)
            {
                ConductBallPhysics(_activeBall, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            // if the ball is done moving, move to the next turn
            if (_activeBall?.CurrentState >= BallObject.State.Done)
            {
                NextTurn();
            }

            UpdateCamera(gameTime);
            AnimatePreviews(gameTime);

            base.Update(gameTime);
        }

        public override void Clean(GameObject gameObject)
        {
            // remove from lists
            if (gameObject is LevelObject levelObject)
            {
                _typeObjects[levelObject.Type].Remove(levelObject);
                _collisionObjects.Remove(levelObject);
            }

            base.Clean(gameObject);
        }

        public void OnBallHit()
        {
            SetBallFollow(true);
        }

        public void OnBallStroke()
        {
            TrimPreviews();
            _strokeText.Content = $"Strokes: {ActivePlayer.Stroke}";
        }

        #region Initializing

        private void LoadInstructions()
        {
            const float margin = 30.0f;

            float layoutWidth = Constants.RESOLUTION_WIDTH - _pauseMenu.LocalSize.X - margin * 2.0f;

            float itemWidth = layoutWidth;
            const float itemHeight = 30.0f;

            // create a layout
            LayoutObject layout = Instantiate(new LayoutObject(new Vector2(layoutWidth, Constants.RESOLUTION_HEIGHT), this)
            {
                CellSize = new Vector2(layoutWidth, itemHeight),
                CellOrientation = LayoutObject.Orientation.Vertical,
            }, new Vector2(_pauseMenu.LocalSize.X + margin, margin), _pauseMenu);

            // load all lines into the layout in text objects
            string path = Path.Combine(Constants.CONTENT_ROOT_DIRECTORY, "LevelInstructions.txt");

            string[] lines = ExternalContent.ReadText(path);

            foreach (string line in lines)
            {
                Instantiate(new TextObject(line, new Vector2(itemWidth, itemHeight), 0.9f, this), layout);
            }

            layout.Refresh();
        }

        #endregion

        #region Pause

        private void SetPause(bool pause)
        {
            if(pause != Paused)
            {
                // pause
                _pauseSfx.Play();
                Paused = pause;
                _pauseMenu.Visible = pause;

                // cancel stroke if taking one
                _activeBall?.CancelStroke();
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
            Instantiate(new ButtonObject("Restart", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                ReloadLevel();
                SetPause(false);
            })
            {
                Depth = pauseMenuItemDepth,
                LocalSize = new Vector2(pauseMenuWidth, 80.0f),
                Margin = 0.0625f,
            }, new Vector2(pauseMenuSpacing, pauseMenuSpacing * 3.0f + 200.0f), _pauseMenu);
            Instantiate(new ButtonObject("Exit", new Sprite(uiTexture, Constants.UI_BUTTON), this, (GameObject _) =>
            {
                Exit();
            })
            {
                Depth = pauseMenuItemDepth,
                LocalSize = new Vector2(pauseMenuWidth, 80.0f),
                Margin = 0.0625f,
            }, new Vector2(pauseMenuSpacing, pauseMenuSpacing * 4.0f + 300.0f), _pauseMenu);

            _pauseMenu.Visible = false;
        }

        #endregion

        #region Camera

        private void SetBallFollow(bool follow)
        {
            _isFollowingBall = follow;
            CanMoveCamera = !follow;
        }

        private Vector2 GetCameraTargetPosition()
        {
            if (_activeBall == null) return LocalPosition;

            return -_activeBall.LocalPosition * LocalScale + CameraOffset + CameraSize * 0.5f;
        }

        private void SnapCameraToBall()
        {
            LocalPosition = GetCameraTargetPosition();
        }

        private void UpdateCamera(GameTime gameTime)
        {
            // get the lower mouse button state
            ButtonState rightMouseButtonState = Input.GetMouseButtonState(Input.MouseButton.Right);
            ButtonState middleMouseButtonState = Input.GetMouseButtonState(Input.MouseButton.Middle);
            ButtonState buttonState = rightMouseButtonState < middleMouseButtonState ? rightMouseButtonState : middleMouseButtonState;

            // stop following if we try to move
            // stop following if 
            if (buttonState == ButtonState.Down)
            {
                SetBallFollow(false);
            }

            // cannot drag if following the ball
            if (_isFollowingBall)
            {
                // lerp to ball
                LocalPosition = Vector2.Lerp(LocalPosition, GetCameraTargetPosition(), Constants.CAMERA_LERP_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        #endregion

        #region Game Management

        private bool EndOfTurn()
        {
            // if there is a ball, handle it
            if (_activeBall != null)
            {
                // act upon the ball
                switch (_activeBall.CurrentState)
                {
                    case BallObject.State.Done:
                        // ball finished turn as normal, nothing crazy happened
                        // update respawn point
                        _alivePlayerSpawns[_activePlayerIndex] = _activeBall.GetData();
                        break;
                    case BallObject.State.Dead:
                        // oops, the ball hit a hazzard
                        break;
                    case BallObject.State.Sunk:
                        // yippee, the ball sunk in a hole!
                        return true;
                    default:
                        // the ball is not done moving: what are we doing here?
                        return false;
                }

                // destroy old ball
                _activeBall.Destroy();
                _activeBall = null;
            }

            return false;
        }

        private void RemoveActivePlayerFromPlay()
        {
            _alivePlayers.RemoveAt(_activePlayerIndex);
            _alivePlayerSpawns.RemoveAt(_activePlayerIndex);
        }

        /// <summary>
        /// Moves to the player/ball
        /// </summary>
        private void NextTurn()
        {
            // ignore if no players
            if (_alivePlayers.Count == 0) return;

            int move = 0;

            if (_activePlayerIndex == -1) // if beginning of the game, move to first player 
            {
                move = 1;
            }
            else if (EndOfTurn()) // if the current player sunk their ball, remove them from play
            {
                _winCount++;
                RemoveActivePlayerFromPlay();
            }
            else if (ActivePlayer.Stroke >= _balls.Count) // if out of strokes, remove them from play
            {
                _loseCount++;
                RemoveActivePlayerFromPlay();
            }
            else // no one was removed, so move to the next player
            {
                move = 1;
            }

            // if no players left, game over
            if (_alivePlayers.Count == 0)
            {
                TriggerGameOver();
                return;
            }

            // move to next player
            int newPlayerIndex = (_activePlayerIndex + move) % _alivePlayers.Count;

            if (_activePlayerIndex != newPlayerIndex)
            {
                _activePlayerIndex = newPlayerIndex;

                // only need to refresh if new, otherwise TrimPreviews takes care of removing balls
                RefreshPreviews();
            }

            _strokeText.Content = $"Strokes: {ActivePlayer.Stroke}";

            // spawn their ball
            _activeBall = SpawnBall(ActivePlayer);
            SetBallFollow(true);
        }

        private void TriggerGameOver()
        {
            _state = State.GameOver;
            _timer = Constants.LEVEL_PAUSE_TIME;
        }

        private void GameOver()
        {
            // if from editor or level select, we are testing: keep restarting
            // or if nobody got to the hole, restart
            if (_exitScene == SceneType.Editor || _exitScene == SceneType.Select || _winCount == 0)
            {
                ReloadLevel();
            }
            else
            {
                NextLevel();
            }
        }

        private void ReloadLevel()
        {
            MiniGolfGame.LoadScene(SceneType.Level, _path, _exitScene);
        }

        private void NextLevel()
        {
            if (!MiniGolfGame.LoadLevel(_worldName, _levelNumber + 1, _exitScene))
            {
                // if could not load the next level, go to the main menu
                // TODO: go to game over scene
                Exit();
            }
        }

        private void Exit()
        {
            switch(_exitScene)
            {
                case SceneType.Editor:
                    MiniGolfGame.LoadEditor(_worldName, _levelNumber);
                    break;
                default:
                    MiniGolfGame.LoadScene(_exitScene);
                    break;
            }            
        }

        #endregion

        #region Objects

        private LevelObject InstantiateLevelObject(LevelObject levelObject, Vector2? position = null, float? rotation = null)
        {
            levelObject = Instantiate(levelObject, position ?? levelObject.LocalPosition, rotation ?? levelObject.LocalRotation);

            // add to type list for quick access
            _typeObjects[levelObject.Type].Add(levelObject);

            // add to collisions if it is collidable and not a ball
            if (levelObject.Flags.HasFlag(BehaviorFlags.Collidable) && levelObject.Type != ObjectType.Ball)
            {
                _collisionObjects.Add(levelObject);
            }

            return levelObject;
        }

        /// <summary>
        /// Spawns a GameComponent using the given type and data.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private LevelObject CreateLevelObject(ObjectTypeData typeData, ObjectData data, Texture2D texture)
        {
            // if a ball, return null, as balls can only be created with SpawnBall
            if (typeData.Type == ObjectType.Ball)
            {
                return null;
            }

            // create object
            LevelObject levelObject = new(typeData, texture, this);

            // set to proper location, size, rot
            levelObject.SetOrientation(data.Position, Vector2.One, data.Size, data.Rotation);

            return levelObject;
        }

        private BallObject SpawnBall(Player player, Vector2? position = null, float? rotation = null)
        {
            BallObject ball = (BallObject)InstantiateLevelObject(new BallObject(_balls[player.Stroke], player, this));

            BallData data = _alivePlayerSpawns[_activePlayerIndex];

            if(position != null)
            {
                data.Position = position.Value;
            }
            if(rotation != null)
            {
                data.Rotation = rotation.Value;
            }

            ball.SetData(data);

            return ball;
        }

        public GameObject[] GetGameObjects(ObjectType type)
        {
            if(_typeObjects.TryGetValue(type, out List<GameObject> value))
            {
                return value.ToArray();
            }

            return Array.Empty<GameObject>();
        }

        #endregion

        #region Physics

        private static BallCollisionData GetBallRectangularCollisionData(Vector2 ballCenter, LevelObject obj)
        {
            // get the obj orientation data
            Hitbox objHitbox = obj.GetHitbox();
            Vector2 objPosition = objHitbox.Position;
            Vector2 objSize = objHitbox.Size;
            float objRotation = objHitbox.Rotation;
            float objRotationInRadians = MathHelper.ToRadians(objRotation);

            // get the distance and angle to the ball from the obj
            float distance = objPosition.DistanceTo(ballCenter);
            float angleToBallInRadians = objPosition.AngleTo(ballCenter);
            //float newAngle = objRotationInRadians - angleToBallInRadians;
            float newAngle = angleToBallInRadians - objRotationInRadians;

            Vector2 relativeBallCenter = new(
                    MathF.Cos(newAngle) * distance,
                    MathF.Sin(newAngle) * distance
                    );

            // get the clamped position within the rect
            Vector2 clampedBallCenter = new(
                Math.Clamp(relativeBallCenter.X, 0.0f, objSize.X),
                Math.Clamp(relativeBallCenter.Y, 0.0f, objSize.Y)
                );

            // if the clamped position is within the radius, they are colliding
            return new BallCollisionData()
            {
                ClampedBallPosition = clampedBallCenter,
                RelativeBallPosition = relativeBallCenter,
            };
        }

        // return true if collided with a solid object
        private bool ConductBallPhysics(BallObject ball, float deltaTime, bool triggerEvents = true)
        {
            // if the ball is not moving, ignore it
            if (ball.CurrentState != BallObject.State.Moving) return false;

            // check for future collisions

            // get ball data
            Vector2 ballVelocity = ball.Velocity * deltaTime;
            float ballRadius = ball.GetGlobalSize().X * 0.5f;
            Vector2 ballCenter = ball.GetGlobalPosition();
            Vector2 ballFutureCenterPosition = ballCenter + ballVelocity;

            bool collided;
            bool solidCollision = false;

            // check against all other collidable objects
            for (int i = _collisionObjects.Count - 1; i >= 0; i--)
            {
                LevelObject obj = _collisionObjects[i];

                if (obj.Flags.HasFlag(BehaviorFlags.Round))
                {
                    // if round, just use radii
                    collided = ballFutureCenterPosition.DistanceTo(obj.GetGlobalCenter()) <= ballRadius + obj.GetGlobalSize().X * 0.5f;
                }
                else
                {
                    // if not round, its more complicated
                    BallCollisionData collisionData = GetBallRectangularCollisionData(ballFutureCenterPosition, obj);

                    collided = collisionData.RelativeBallPosition.DistanceTo(collisionData.ClampedBallPosition) <= ballRadius;
                }

                ball.TestObject(obj, collided);

                // if within the radius of the ball, collison is going to occur
                if (collided)
                {
                    // determine what to do based on the other object's properties
                    // if PreCollideWith true, the ball should reflect if solid
                    if (ball.PreCollideWith(obj))
                    {
                        solidCollision = obj.Flags.HasFlag(BehaviorFlags.Solid);
                        if (solidCollision)
                        {
                            // TODO: account for round reflection
                            //if(obj.Flags.HasFlag(BehaviorFlags.Round))
                            //{

                            //}

                            // calulcate the normal for reflection

                            // angle from clamped to relative ball center will be the normal
                            // then also reverse the previous angle
                            Vector2 normal;

                            if (obj.Flags.HasFlag(BehaviorFlags.Round))
                            {
                                // if round, just get direction to future center
                                normal = obj.GetGlobalCenter().DirectionTo(ballFutureCenterPosition);
                            }
                            else
                            {
                                // get the positional data to do the collision
                                BallCollisionData collisionData = GetBallRectangularCollisionData(ballCenter, obj);

                                normal = collisionData.ClampedBallPosition.DirectionTo(collisionData.RelativeBallPosition);

                                // now rotate the normal back to world space instead of local space
                                normal = Vector2Helper.FromAngle(Vector2Helper.Angle(Vector2.Zero, normal) + MathHelper.ToRadians(obj.GetGlobalRotation()));
                            }

                            // reflect using the normal given from the collision data
                            ball.Reflect(normal);

                            if(obj.Flags.HasFlag(BehaviorFlags.Bouncy))
                            {
                                // bounce in the direction of the normal
                                ball.Velocity += Constants.LEVEL_BOUNCE_POWER * normal;

                                // start animation
                                obj.StartTimer(Constants.LEVEL_BOUNCE_TIME);
                            }
                        }

                        if (triggerEvents)
                        {
                            // decide what to do when colliding with something
                            ball.CollideWith(obj, deltaTime);
                        }

                        // only one solid collision per frame
                        if (solidCollision)
                        {
                            break;
                        }
                    }
                }
            }

            // done with colliding, now move as normal
            ball.Move(deltaTime);

            return solidCollision;
        }

        public void ThwackBall(BallObject ball)
        {
            // do nothing if no ball or if ball is not moving
            if (ball == null || !ball.IsMoving) return;

            // get ideal deltaTime so ball moves incrementally (not too fast or slow)
            float gameTime = ball.LocalSize.X / ball.Velocity.Magnitude();

            Vector2 initialPosition = ball.LocalPosition;
            Vector2 initialVelocity = ball.Velocity;

            // simulate for a long time, or until a collision
            for (int i = 0; i < Constants.BALL_THWACK_ITERATIONS; i++)
            {
                if (ConductBallPhysics(ball, gameTime, false))
                {
                    // ball collided

                    // spawn a visual
                    Instantiate(new ThwackObject(initialPosition, ball.LocalPosition, ball.LocalSize.X * 0.9f, this));

                    return;
                }

                // might have slown down from friction, etc. so reset velocity
                ball.Velocity = initialVelocity;
            }

            // if nothing was hit, move back and hit as normal. That was a pathetic hit.
            ball.LocalPosition = initialPosition;
            ball.Velocity = initialVelocity;
        }

        #endregion

        #region Previews

        private void TrimPreviews()
        {
            int offset = ActivePlayer.Stroke;
            int count = _balls.Count - offset;

            for (int i = 0; i < _ballPreviews.Count - count; i++)
            {
                _ballPreviews[0].Destroy();
                _ballPreviews.RemoveAt(0);
            }
        }

        private const float PREVIEW_OFFSET = 45.0f;

        private void RefreshPreviews()
        {
            // destroy old previews
            for (int i = _ballPreviews.Count - 1; i >= 0; i--)
            {
                _ballPreviews[i].Destroy();
            }
            _ballPreviews.Clear();

            // create new ones based on the stroke
            int offset = ActivePlayer.Stroke;
            int count = _balls.Count - offset;

            for (int i = 0; i < count; i++)
            {
                SpriteObject preview = new(new Sprite(Content.Load<Texture2D>($"Texture/{_balls[i + offset]}"), null, new Vector2(0.5f, 0.5f)), this)
                {
                    Depth = 0.9f, // render on top
                    LocalPosition = new Vector2(PREVIEW_OFFSET + i * 40.0f, PREVIEW_OFFSET),
                    LocalSize = new Vector2(50.0f),
                };
                Instantiate(preview, _canvas);
                _ballPreviews.Add(preview);
            }
        }

        private void AnimatePreviews(GameTime gameTime)
        {
            // skip if no balls
            if (!_ballPreviews.Any()) return;

            // only care about first ball since they move in unison
            SpriteObject firstBall = _ballPreviews[0];

            if (firstBall.LocalPosition.X > PREVIEW_OFFSET)
            {
                // move all balls
                Vector2 move = new(Constants.PREVIEW_ANIMATION_SPEED * (float)gameTime.ElapsedGameTime.TotalSeconds, 0.0f);
                float rotation = move.X * firstBall.LocalSize.X * 0.5f * Constants.BALL_SPIN_SCALE;

                foreach (SpriteObject preview in _ballPreviews)
                {
                    preview.LocalPosition -= move;
                    preview.LocalRotation -= rotation;
                }

                // if first is done, they all are
                if (firstBall.LocalPosition.X < PREVIEW_OFFSET)
                {
                    for (int i = 0; i < _ballPreviews.Count; i++)
                    {
                        _ballPreviews[i].LocalPosition = new Vector2(PREVIEW_OFFSET + i * 40.0f, PREVIEW_OFFSET);
                    }
                }
            }
        }

        #endregion
    }
}
