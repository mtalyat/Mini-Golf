using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MiniGolf
{
    /// <summary>
    /// The scene for running a level.
    /// </summary>
    internal class LevelScene : Scene
    {
        private LevelData _data;
        private readonly int _levelId;
        private Texture2D _levelTexture;

        private readonly List<BallType> _balls = new();

        private readonly Dictionary<ObjectType, List<GameObject>> _typeObjects = new();

        private readonly List<LevelObject> _collisionObjects = new();

        // player management data:
        private List<Player> _alivePlayers;
        private List<Vector2> _alivePlayerSpawns;
        private int _activePlayerIndex = -1;
        private Player ActivePlayer => _alivePlayers[_activePlayerIndex];

        private BallObject _activeBall = null;

        public LevelScene(int levelId, Game game) : base(game)
        {
            _levelId = levelId;
            _alivePlayers = new List<Player>(Session.Players);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            string path = $"Level/Level{_levelId}/";

            // load level data from the file
            _data = new LevelData($"Content/{path}data.txt");

            // get the ball names, put them in a list so they can easily be grabbed
            foreach (string name in _data.TakeValue("Balls").Split(' '))
            {
                _balls.Add(Enum.Parse<BallType>(name));
            }

            // load the level png
            _levelTexture = Game.Content.Load<Texture2D>($"{path}level");

            // load the level components png
            Texture2D levelComponentsTexture = Game.Content.Load<Texture2D>($"{path}components");

            // create each object
            foreach (var pair in _data.ObjectDatas)
            {
                // add a new list for the type
                _typeObjects.Add(pair.Key.Type, new List<GameObject>());

                foreach (var data in pair.Value)
                {
                    // create object using type and other data
                    InstantiateLevelObject(CreateLevelObject(pair.Key, data, levelComponentsTexture));
                }
            }

            // ensure ball is in there
            _typeObjects.TryAdd(ObjectType.Ball, new List<GameObject>());

            // find the starting spawn location
            Vector2 start = _typeObjects[ObjectType.Start][0].GetGlobalPosition();
            // populate the respawn points
            _alivePlayerSpawns = new List<Vector2>(Enumerable.Repeat(start, _alivePlayers.Count));

            // start the game
            NextTurn();

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // if the ball is in the scene, simulate physics on it
            if (_activeBall != null)
            {
                ConductBallPhysics(gameTime);
            }

            // if the ball is done moving, move to the next turn
            if(_activeBall?.CurrentState >= BallObject.State.Done)
            {
                NextTurn();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();

            // draw the background
            SpriteBatch.Draw(_levelTexture, new Vector2(0.0f, 0.0f), Color.White);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

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
                        _alivePlayerSpawns[_activePlayerIndex] = _activeBall.GetGlobalPosition();
                        break;
                    case BallObject.State.Dead:
                        // oops, the ball hit a hazzard, add a stroke
                        ActivePlayer.Stroke += 1;
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
            int move = 0;

            if(_activePlayerIndex == -1) // if beginning of the game, move to first player 
            {
                move = 1;
            }
            else if(EndOfTurn()) // if the current player sunk their ball, remove them from play
            {
                RemoveActivePlayerFromPlay();
            }
            else if(ActivePlayer.Stroke >= _balls.Count) // if out of strokes, remove them from play
            {
                RemoveActivePlayerFromPlay();
            }
            else // no one was removed, so move to the next player
            {
                move = 1;
            }

            // if no players left, game over
            if(_alivePlayers.Count == 0)
            {
                GameOver();
                return;
            }

            // move to next player
            _activePlayerIndex = (_activePlayerIndex + move) % _alivePlayers.Count;

            // spawn their ball
            _activeBall = SpawnBall(ActivePlayer);
        }
        
        private void GameOver()
        {
            ((MiniGolfGame)Game).LoadScene(SceneType.MainMenu);
        }

        #endregion

        #region Creating Objects

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
            // create object
            LevelObject levelObject = typeData.Type switch
            {
                ObjectType.Ball => new LevelObject
                    (
                    typeData.Type,
                    new Sprite(Content.Load<Texture2D>("Objects/GolfBall"), null),
                    this
                    ),
                _ => new LevelObject
                    (
                    typeData.Type,
                    new Sprite(texture, typeData.Rect, typeData.Pivot),
                    this
                    ),
            };

            // set to proper location, size, rot
            levelObject.SetOrientation(data.Position, Vector2.One, data.Size, data.Rotation);

            return levelObject;
        }

        private BallObject SpawnBall(Player player, Vector2? position = null)
        {
            if (position == null)
            {
                position = _alivePlayerSpawns[_activePlayerIndex];
            }

            return (BallObject)InstantiateLevelObject(new BallObject(_balls[player.Stroke], player, this), position.Value);
        }

        #endregion

        #region Physics

        private static BallCollisionData GetBallCollisionData(Vector2 ballCenter, LevelObject obj)
        {
            // TODO: differentiate round from square
            //if(obj.Flags.HasFlag(BehaviorFlags.Round))
            //{
            //    // the object is round

            //}
            //else
            //{
            //    // the object is a square
            //}

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

        private void ConductBallPhysics(GameTime gameTime)
        {
            // get elapsed time as a float
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // check for future collisions

            // get ball data
            Hitbox ballHitbox = _activeBall.GetHitbox();
            Vector2 ballPosition = ballHitbox.Position;
            Vector2 ballVelocity = _activeBall.Velocity * deltaTime;
            float ballRadius = _activeBall.Radius;
            Vector2 ballCenter = ballPosition + new Vector2(ballRadius, ballRadius);
            Vector2 ballFutureCenterPosition = ballCenter + ballVelocity;

            // check against all other collidable objects
            foreach (LevelObject obj in _collisionObjects)
            {
                // get collision data for new position
                BallCollisionData collisionData = GetBallCollisionData(ballFutureCenterPosition, obj);

                // if within the radius of the ball, collison is going to occur
                if (collisionData.RelativeBallPosition.DistanceTo(collisionData.ClampedBallPosition) <= ballRadius)
                {
                    // determine what to do based on the other object's properties

                    // if solid, reflect
                    if (obj.Flags.HasFlag(BehaviorFlags.Solid))
                    {
                        // TODO: account for round reflection
                        //if(obj.Flags.HasFlag(BehaviorFlags.Round))
                        //{

                        //}

                        // get the positional data to do the collision
                        collisionData = GetBallCollisionData(ballCenter, obj);

                        // calulcate the normal for reflection

                        // angle from clamped to relative ball center will be the normal
                        // then also reverse the previous angle
                        Vector2 normal = collisionData.ClampedBallPosition.DirectionTo(collisionData.RelativeBallPosition);

                        // now rotate the normal back to world space instead of local space
                        normal = Vector2Helper.FromAngle(Vector2Helper.Angle(Vector2.Zero, normal) + MathHelper.ToRadians(obj.GetGlobalRotation()));

                        // reflect using the normal given from the collision data
                        _activeBall.Reflect(normal);
                    }

                    // decide what to do when colliding with something
                    _activeBall.CollideWith(obj, deltaTime);

                    // only one collision per frame
                    break;
                }
            }

            // done with colliding, now move as normal
            _activeBall?.Move(deltaTime);
        }

        #endregion
    }
}
