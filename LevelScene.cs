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
        private Texture2D _backgroundTexture;

        private readonly List<BallType> _balls = new();

        private readonly Dictionary<ObjectType, List<GameObject>> _typeObjects = new();

        private readonly List<LevelObject> _collisionObjects = new();

        // player management data:
        // holds references to player objects
        private List<Player> _alivePlayers;
        // holds position and rotation
        private List<Vector3> _alivePlayerSpawns;
        // current player index in _alivePlayers
        private int _activePlayerIndex = -1;
        // active player with the active ball
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
            string path = $"Scene/World1/";

            // load level data from the file
            _data = new LevelData($"Content/{path}level{_levelId}.txt");

            // get the ball names, put them in a list so they can easily be grabbed
            foreach (string name in _data.TakeValue("Balls").Split(' '))
            {
                _balls.Add(Enum.Parse<BallType>(name));
            }

            // load the level png
            _backgroundTexture = Game.Content.Load<Texture2D>($"{path}background");

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
            Vector2 start = _typeObjects[ObjectType.Start].First().GetGlobalCenter();
            // populate the respawn points
            _alivePlayerSpawns = new List<Vector3>(Enumerable.Repeat(new Vector3(start, 0.0f), _alivePlayers.Count));

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
                ConductBallPhysics(_activeBall, (float)gameTime.ElapsedGameTime.TotalSeconds);
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
            SpriteBatch.Draw(_backgroundTexture, new Vector2(0.0f, 0.0f), Color.White);

            SpriteBatch.End();

            base.Draw(gameTime);
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
                        _alivePlayerSpawns[_activePlayerIndex] = new Vector3(_activeBall.GetGlobalPosition(), _activeBall.GetGlobalRotation());
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

        private BallObject SpawnBall(Player player, Vector2? position = null, float? rotation = null)
        {
            Vector3 spawn = _alivePlayerSpawns[_activePlayerIndex];

            position ??= new Vector2(spawn.X, spawn.Y);
            rotation ??= spawn.Z;

            return (BallObject)InstantiateLevelObject(new BallObject(_balls[player.Stroke], player, this), position.Value, rotation);
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
            // check for future collisions

            // get ball data
            Vector2 ballVelocity = ball.Velocity * deltaTime;
            float ballRadius = ball.Radius;
            Vector2 ballCenter = ball.GetGlobalPosition();
            Vector2 ballFutureCenterPosition = ballCenter + ballVelocity;

            bool collided = false;
            bool solidCollision = false;

            // check against all other collidable objects
            foreach (LevelObject obj in _collisionObjects)
            {
                if(obj.Flags.HasFlag(BehaviorFlags.Round))
                {
                    // if round, just use radii
                    collided = ballFutureCenterPosition.DistanceTo(obj.GetGlobalCenter()) <= ballRadius + obj.GetGlobalSize().X / 2.0f;
                }
                else
                {
                    // if not round, its more complicated
                    BallCollisionData collisionData = GetBallRectangularCollisionData(ballFutureCenterPosition, obj);

                    collided = collisionData.RelativeBallPosition.DistanceTo(collisionData.ClampedBallPosition) <= ballRadius;
                }

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
                        }

                        if(triggerEvents)
                        {
                            // decide what to do when colliding with something
                            ball.CollideWith(obj, deltaTime);
                        }
                    }

                    // only one collision per frame
                    break;
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
            float gameTime = ball.Radius * 2.0f / ball.Velocity.Magnitude();

            Vector2 initialPosition = ball.LocalPosition;
            Vector2 initialVelocity = ball.Velocity;

            // simulate for a long time, or until a collision
            for (int i = 0; i < Constants.BALL_THWACK_ITERATIONS; i++)
            {
                if(ConductBallPhysics(ball, gameTime, false))
                {
                    // ball collided

                    // spawn a visual
                    Instantiate(new ThwackObject(initialPosition, ball.GetGlobalCenter(), ball.Radius * 1.8f, this));

                    return;
                }

                // might have slown down from friction, etc. so reset velocity
                ball.Velocity = initialVelocity;
            }

            // if nothing was hit, move back and hit as normal. That was a pathetic hit.
            ball.LocalPosition = initialPosition;
            ball.Velocity = initialVelocity;
        }

        public RaycastHit Raycast(Vector2 origin, Vector2 direction, float maxDistance = float.MaxValue) => Raycast(new Ray(origin, direction), maxDistance);

        public RaycastHit Raycast(Ray ray, float maxDistance = float.MaxValue) => Raycast(ray, _collisionObjects, maxDistance);

        public RaycastHit Raycast(Ray ray, List<LevelObject> levelObjects, float maxDistance = float.MaxValue)
        {
            LevelObject closest = null;
            float closestDistance = maxDistance;

            foreach (LevelObject levelObject in levelObjects)
            {
                // if the level object is not solid, ignore
                if (!levelObject.Flags.HasFlag(BehaviorFlags.Solid))
                {
                    continue;
                }

                if (CheckIfRayIntersectsLevelObject(ray, levelObject, out float distance) && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = levelObject;
                }
            }

            if(closest != null)
            {
                return new RaycastHit(ray);
            }
            else
            {
                // TODO: get normal
                return new RaycastHit(ray, closest, closestDistance, new Vector2());
            }
        }

        private static bool CheckIfRayIntersectsLevelObject(Ray ray, LevelObject obj, out float distance)
        {
            distance = 0.0f;

            Vector2 iDir = new Vector2(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y);

            Hitbox hitbox = obj.GetHitbox();
            (Vector2 topLeft, Vector2 bottomRight) = hitbox.GetMinMax();

            Vector2 min = (topLeft - ray.Origin) * iDir;
            Vector2 max = (bottomRight - ray.Origin) * iDir;

            Vector2 tMin = Vector2.Min(min, max);
            Vector2 tMax = Vector2.Max(min, max);

            float t0 = MathF.Max(tMin.X, tMin.Y);
            float t1 = MathF.Min(tMax.X, tMax.Y);

            if(t0 > t1 || t1 < 0)
            {
                return false;
            }

            distance = t0 >= 0 ? t0 : t1;
            return true;
        }

        #endregion
    }
}
