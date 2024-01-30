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

        private readonly List<string> _ballNames = new();

        private readonly Dictionary<ObjectType, List<GameObject>> _typeObjects = new();

        private readonly List<LevelObject> _collisionObjects = new();

        private BallObject _activeBall = null;

        public LevelScene(int levelId, Game game) : base(game)
        {
            _levelId = levelId;
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

            // get the ball names, put them in a queue so they can easily be grabbed, and load the textures while we're at it
            foreach (string name in _data.TakeValue("Balls").Split(' '))
            {
                _ballNames.Add(name);
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

            _typeObjects.TryAdd(ObjectType.Ball, new List<GameObject>());
            _activeBall = SpawnBall(new Player("Dork", Color.White));

            base.LoadContent();
        }

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

        private BallObject SpawnBall(Player player, Vector2? position = null)
        {
            if (position == null)
            {
                position = _typeObjects[ObjectType.Start].First().GetGlobalPosition();
            }

            return (BallObject)InstantiateLevelObject(new BallObject(Enum.Parse<BallType>(_ballNames[player.Stroke]), player, this), position.Value);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

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
                        _activeBall.Velocity = _activeBall.Velocity - 2.0f * Vector2.Dot(_activeBall.Velocity, normal) * normal;
                    }

                    // determine what to do based on its type
                    switch (obj.Type)
                    {
                        case ObjectType.Hole:
                            // if the center of the ball is over the hole, sink the ball
                            // TODO: sink, not destroy
                            if (obj.GetHitbox().ContainsRound(_activeBall.GetGlobalCenter()))
                            {
                                _activeBall.Destroy();
                                _activeBall = null;
                            }
                            break;
                        case ObjectType.WallDamaged:
                            // break if heavy or if football
                            if (_activeBall.WeightClass == BallObject.Weight.Heavy || _activeBall.BallType == BallType.FootballBall)
                            {
                                obj.Destroy();

                                // if not football, slow down slightly
                                if (_activeBall.BallType != BallType.FootballBall)
                                {
                                    _activeBall.SlowDown(Constants.LEVEL_DAMAGED_WALL_SLOW_DOWN * deltaTime);
                                }
                            }
                            break;
                        case ObjectType.Wall:
                        case ObjectType.Wall1:
                        case ObjectType.Wall2:
                        case ObjectType.Wall3:
                        case ObjectType.Wall4:
                            break;
                        case ObjectType.Slope:
                            // get vector direction of slope from its angle, add to velocity
                            _activeBall.Velocity += Vector2Helper.FromAngle(MathHelper.ToRadians(obj.GetGlobalRotation())) * Constants.LEVEL_SLOPE_FORCE * deltaTime;
                            break;
                        case ObjectType.Hill:
                            // get vector direction of slope from center of the hill, add to velocity
                            _activeBall.Velocity += Vector2Helper.FromAngle(Vector2Helper.Angle(obj.GetGlobalCenter(), ballCenter)) * Constants.LEVEL_SLOPE_FORCE * deltaTime;
                            break;
                        case ObjectType.Valley:
                            // get vector direction of slope from center of the hill, subtract from velocity
                            _activeBall.Velocity -= Vector2Helper.FromAngle(Vector2Helper.Angle(obj.GetGlobalCenter(), ballCenter)) * Constants.LEVEL_SLOPE_FORCE * deltaTime;
                            break;
                        case ObjectType.Sandbar:
                            // slow down the ball
                            _activeBall.Velocity *= 1.0f - Math.Min(Constants.LEVEL_SANDBAR_FORCE * deltaTime, 1.0f);
                            break;
                        case ObjectType.Water:
                            // kill ball
                            _activeBall.Destroy();
                            _activeBall = null;
                            break;
                        case ObjectType.Box:
                            // kill box if heavy
                            if (_activeBall.WeightClass == BallObject.Weight.Heavy)
                            {
                                obj.Destroy();
                            }
                            break;
                    }

                    // only one collision per frame
                    break;
                }
            }

            // done with colliding, now move as normal
            _activeBall?.Move(deltaTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (_activeBall != null)
            {
                ConductBallPhysics(gameTime);
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
    }
}
