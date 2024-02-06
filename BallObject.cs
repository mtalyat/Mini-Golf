using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class BallObject : LevelObject
    {
        public enum State
        {
            /// <summary>
            /// The ball is sitting in place.
            /// </summary>
            Idle,

            /// <summary>
            /// The ball is moving.
            /// </summary>
            Moving,

            /// <summary>
            /// The ball is moving into a portal.
            /// </summary>
            TeleportingIn,

            /// <summary>
            /// The ball is moving out of a portal.
            /// </summary>
            TeleportingOut,

            /// <summary>
            /// The ball is broken.
            /// </summary>
            Broken,

            /// <summary>
            /// The ball is dying.
            /// </summary>
            Dying,

            /// <summary>
            /// The ball is sinking into a hole.
            /// </summary>
            Sinking,

            /// <summary>
            /// The ball is done moving.
            /// </summary>
            Done,

            /// <summary>
            /// The ball is dead.
            /// </summary>
            Dead,

            /// <summary>
            /// The ball has been sunk in a hole.
            /// </summary>
            Sunk,
        }

        public enum Weight
        {
            Light = 1,
            Normal = 2,
            Heavy = 3,
        }

        private readonly Player _owner;
        public Player Owner => _owner;

        private readonly Weight _weight;
        public Weight WeightClass => _weight;

        private readonly float _objectFriction;

        private State _state = State.Idle;
        public State CurrentState => _state;

        private readonly BallType _ballType;
        public BallType BallType => _ballType;

        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public float Speed => Velocity.Magnitude();
        public bool IsMoving => Velocity.X != 0.0f && Velocity.Y != 0.0f;

        private float _angularVelocity = 0.0f;
        private bool _shouldSpin = false;

        private TrailObject _trail = null;
        private AimingObject _aiming = null;

        private LevelObject _portal = null;
        private float _timer;
        private float _maxTime;
        private Vector2 _start;
        private Vector2 _end;

        private SoundEffect _hitSfx;
        private SoundEffect _thwackSfx;
        private SoundEffect _collisionSfx;

        private LevelScene LevelScene => (LevelScene)Scene;

        public BallObject(BallType type, Player owner, Scene scene) : base(ObjectType.Ball, new Sprite(scene.Content.Load<Texture2D>($"Texture/{type}"), null, new Vector2(0.5f, 0.5f)), scene)
        {
            _owner = owner;
            _ballType = type;
            LocalSize = new Vector2(40.0f, 40.0f);
            Depth = 0.5f;

            // set sprite color to owner color
            Color = owner.Color;

            // set weight based on ball type
            switch (type)
            {
                case BallType.PongBall:
                    _weight = Weight.Light;
                    break;
                case BallType.BowlingBall:
                    _weight = Weight.Heavy;
                    break;
                default:
                    _weight = Weight.Normal;
                    break;
            }

            // set friction based on ball type
            switch (type)
            {
                case BallType.SoccerBall:
                    _objectFriction = Constants.BALL_FRICTION * 2.0f;
                    break;
                default:
                    _objectFriction = Constants.BALL_FRICTION;
                    break;
            }
        }

        protected override void LoadContent()
        {
            _hitSfx = Scene.Content.Load<SoundEffect>("Audio/Hit");
            _thwackSfx = Scene.Content.Load<SoundEffect>("Audio/Thwack");
            _collisionSfx = Scene.Content.Load<SoundEffect>($"Audio/{_ballType}");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // if no longer moving and was moving, mark as done
            if (_state == State.Moving)
            {
                // if velocity is > threshold, reset timer
                if (Velocity.Magnitude() > Constants.BALL_STOP_THRESHOLD)
                {
                    _timer = Constants.BALL_STOP_TIME;
                }
                else if (_timer <= 0.0f)
                {
                    // ball has stopped
                    _timer = 0.0f;
                    Stop();

                    // no longer moving
                    _state = State.Done;
                }
                else
                {
                    _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (_shouldSpin && IsMoving)
                {
                    // spin the ball, based on X velocity
                    float spin = _angularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (Velocity.X >= 0.0f)
                    {
                        LocalRotation += spin;
                    }
                    else
                    {
                        LocalRotation -= spin;
                    }
                }
            }

            // if not moving and self was clicked, spawn a trail
            if (_state == State.Idle)
            {
                ButtonState state = Input.GetMouseButtonState(0);
                if (state == ButtonState.Down && Input.ContainsMouse(GetHitbox()))
                {
                    // spawn a trail to the mouse
                    _trail = Scene.Instantiate(new TrailObject(LocalPosition, LocalSize.X, Scene));

                    // set depth to render right under the ball
                    _trail.Depth = Depth - 0.0001f;

                    // if a pool ball, spawn an aiming object
                    if (_ballType == BallType.PoolBall)
                    {
                        _aiming = Scene.Instantiate(new AimingObject(LocalPosition, Scene));
                        _aiming.Depth = Depth - 0.0001f;
                    }
                }
                else if (_trail != null)
                {
                    if (state == ButtonState.Pressed)
                    {
                        // if button held, make ball face the angle of the trail
                        LocalRotation = MathHelper.ToDegrees(GetTrailAngle());
                    }
                    else if (state == ButtonState.Up)
                    {
                        // if button released, hit and destroy the trail
                        Vector2 direction = Vector2Helper.FromAngle(GetTrailAngle());
                        // only hit if strong enough
                        float power = _trail.LocalSize.Y;
                        if(power > LocalSize.X * 0.5f)
                        {
                            Hit(direction * power);
                        }

                        // destroy trail and aiming regardless
                        _trail.Destroy();
                        _trail = null;
                        _aiming?.Destroy();
                        _aiming = null;
                    }
                }
            }

            if (_state >= State.TeleportingIn && _state <= State.Sinking)
            {
                // shrinking
                _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // update scale
                float percent = MathF.Max(_timer, 0.0f) / _maxTime;
                float invertedPercent = 1.0f - percent;
                float scale = percent;

                // update color and position and more
                switch(_state)
                {
                    case State.TeleportingIn:
                        {
                            // spin in to center of target (_end)
                            float angle = _end.AngleTo(_start) + Constants.BALL_TELEPORT_SPIN_AMOUNT * invertedPercent;
                            float distance = _end.DistanceTo(_start) * percent;

                            LocalPosition = _end + Vector2Helper.FromAngle(angle) * distance;
                            LocalScale = new Vector2(percent);
                        }
                        break;
                    case State.TeleportingOut:
                        {
                            // spin out to target position (_end)
                            float angle = _start.AngleTo(_end) - Constants.BALL_TELEPORT_SPIN_AMOUNT * percent;
                            float distance = _start.DistanceTo(_end) * invertedPercent;

                            LocalPosition = _start + Vector2Helper.FromAngle(angle) * distance;
                            LocalScale = new Vector2(invertedPercent);
                        }
                        break;
                    case State.Broken:
                        break;
                    case State.Dying:
                        LocalScale = new Vector2(scale);
                        break;
                    case State.Sinking:
                        LocalScale = new Vector2(scale);
                        LocalPosition = Vector2.Lerp(_end, _start, percent);
                        Color = new Color(percent, percent, percent, 1.0f);
                        break;
                }

                // if timer is done, finish
                if (_timer <= 0.0f)
                {
                    switch(_state)
                    {
                        case State.TeleportingIn:
                            // move to teleport out state
                            _state = State.TeleportingOut;
                            _timer = _maxTime;

                            // find the next portal
                            List<LevelObject> portals = LevelScene.GetGameObjects(_portal.Type).Cast<LevelObject>().ToList();

                            // remove current target so we do not teleport to that one
                            portals.Remove(_portal);

                            // randomly select one of the rest
                            // if none left, use target I guess...
                            if(portals.Any())
                            {
                                _portal = portals[Random.Shared.Next(portals.Count)];
                            }

                            // set new start and stop
                            Vector2 offset = _start - _end;
                            _start = _portal.LocalPosition;
                            _end = _start + offset;
                            break;
                        case State.TeleportingOut:
                            // all done, keep moving
                            _state = State.Moving;
                            break;
                        case State.Dying:
                            _state = State.Dead;
                            break;
                        case State.Sinking:
                            _state = State.Sunk;
                            break;
                        case State.Broken:
                            // go back to normal
                            _state = State.Done;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override Vector2 GetGlobalCenter()
        {
            Vector2 size = GetGlobalSize();
            return GetGlobalPosition() + (size * 0.5f - size * Sprite.Pivot);
        }

        protected override Vector2 GetCenter()
        {
            return LocalPosition + (LocalSize * LocalScale * 0.5f);
        }

        #region Trail

        private float GetTrailAngle()
        {
            return MathHelper.ToRadians(_trail.GetGlobalRotation()) - MathF.PI / 2.0f;
        }

        #endregion

        #region Collisions

        /// <summary>
        /// Tests the given object, and performs some action based on different criteria.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="collision"></param>
        public void TestObject(LevelObject obj, bool collision)
        {
            if(!collision && obj == _portal)
            {
                // no longer inside of the portal
                _portal = null;
            }
        }

        public bool PreCollideWith(LevelObject obj)
        {
            bool nothingHappened = true;

            bool smash = false;

            switch (obj.Type)
            {
                case ObjectType.WallDamaged:
                    // break if heavy or if a football that is not spinning
                    if (_weight == Weight.Heavy || (_ballType == BallType.FootballBall && !_shouldSpin))
                    {
                        smash = true;

                        // if not football, slow down slightly
                        if (BallType != BallType.FootballBall)
                        {
                            SlowDown(Constants.LEVEL_DAMAGED_WALL_SLOW_DOWN);
                        }

                        nothingHappened = false;
                    }
                    break;
                case ObjectType.Box:
                    // kill box if heavy
                    if (_weight == Weight.Heavy)
                    {
                        smash = true;

                        // slow down slightly
                        SlowDown(Constants.LEVEL_BOX_SLOW_DOWN);

                        nothingHappened = false;
                    }
                    break;
            }

            if(smash)
            {
                // play the sound of the obj being smashed
                obj.PlaySound();

                // spawn timed object where obj is
                Scene.Instantiate(new TimedObject(Constants.LEVEL_SMASHED_TIME, null, new Sprite(Scene.Content.Load<Texture2D>("Texture/Smashed"), null, obj.Sprite.Pivot), Scene)
                {
                    LocalPosition = obj.LocalPosition,
                    LocalSize = obj.LocalSize,
                    LocalScale = obj.LocalScale,
                });

                // destroy obj
                obj.Destroy();
            }

            return nothingHappened;
        }

        public void CollideWith(LevelObject obj, float deltaTime)
        {
            // check if this is inside of the object
            bool contains = obj.Flags.HasFlag(BehaviorFlags.Round) ? obj.GetHitbox().ContainsRound(GetGlobalCenter()) : obj.GetHitbox().Contains(GetGlobalCenter());

            // play the custom collision sound, or the default if not available
            if(obj.Flags.HasFlag(BehaviorFlags.Solid) && (!obj.Flags.HasFlag(BehaviorFlags.Sound) || !obj.PlaySound()))
            {
                // default sound
                _collisionSfx.Play();
            }

            // determine what to do based on its type
            switch (obj.Type)
            {
                case ObjectType.Wall:
                case ObjectType.WallDamaged:
                case ObjectType.Wall1:
                case ObjectType.Wall2:
                case ObjectType.Wall3:
                case ObjectType.Wall4:
                case ObjectType.Box:
                case ObjectType.Crate:
                    // if a mint, break (hard object collision)
                    // if not a mint, spin if not already
                    if(_ballType== BallType.Mint)
                    {
                        Break();
                    }
                    else
                    {
                        // spin if hit a wall
                        _shouldSpin = true;
                    }
                    break;
                case ObjectType.Hole:
                    // if the ball is over the hole, sink into it
                    if (contains && Velocity.Magnitude() <= Constants.BALL_SINK_THRESHOLD)
                        Sink(obj);
                    break;
                case ObjectType.Slope:
                    // get vector direction of slope from its angle, add to velocity
                    if (contains)
                        Push(Vector2Helper.FromAngle(MathHelper.ToRadians(obj.GetGlobalRotation())) * Constants.LEVEL_SLOPE_FORCE * deltaTime);
                    break;
                case ObjectType.Hill:
                    // get vector direction of slope from center of the hill, add to velocity
                    if (contains)
                        Push(Vector2Helper.FromAngle(Vector2Helper.Angle(obj.GetGlobalCenter(), GetGlobalCenter())) * Constants.LEVEL_SLOPE_FORCE * deltaTime);
                    break;
                case ObjectType.Valley:
                    // get vector direction of slope from center of the hill, subtract from velocity
                    if (contains && GetGlobalCenter().DistanceTo(obj.GetGlobalCenter()) > 4.0f) // give a small center of no force so balls can settle
                        Push(-Vector2Helper.FromAngle(Vector2Helper.Angle(obj.GetGlobalCenter(), GetGlobalCenter())) * Constants.LEVEL_SLOPE_FORCE * deltaTime);
                    break;
                case ObjectType.Sand:
                    // slow down the ball, if not a light ball
                    if (_weight != Weight.Light && contains)
                    {
                        Velocity *= 1.0f - Math.Min(Constants.LEVEL_SANDBAR_FORCE * deltaTime, 1.0f);
                    }
                    break;
                case ObjectType.Water:
                    // kill ball if center is in the hazard
                    if (contains)
                        Die(obj);
                    break;
                case ObjectType.Portal1:
                case ObjectType.Portal2:
                case ObjectType.Portal3:
                    if (contains)
                        Teleport(obj);
                    break;
            }
        }

        #endregion

        #region Movement

        public void Hit(Vector2 directionAndPower)
        {
            LevelScene levelScene = (LevelScene)Scene;

            // move the ball
            Velocity = Constants.BALL_HIT_POWER * (2.0f - (float)_weight / 2.0f) * directionAndPower;
            // mark as moving
            _state = State.Moving;
            // mark as spinning if not a football
            _shouldSpin = _ballType != BallType.FootballBall;
            // add a stroke if not a mint (mint adds when it breaks)
            if(_ballType != BallType.Mint)
            {
                _owner.Stroke++;
                levelScene.OnBallStroke();
            }

            levelScene.OnBallHit();

            // if hockey puck, thwack it
            if (_ballType == BallType.HockeyPuck)
            {
                // thwack ball
                levelScene.ThwackBall(this);

                // thwack sound
                _thwackSfx.Play();
            } 
            else
            {
                // normal hit sound
                _hitSfx.Play();
            }
        }

        public void Push(Vector2 directionAndPower)
        {
            // soccer ball not affected by pushing
            if (_ballType != BallType.SoccerBall)
            {
                Velocity += directionAndPower;
            }
        }

        public void Stop()
        {
            Velocity = Vector2.Zero;
        }

        public void SlowDown(float percent)
        {
            Velocity *= 1.0f - Math.Clamp(percent, 0.0f, 1.0f);
        }

        public void Move(float deltaTime)
        {
            // only move if in the moving state
            if (_state != State.Moving)
            {
                return;
            }

            // slow from friction
            SlowDown(_objectFriction * deltaTime);

            // move
            LocalPosition += Velocity * deltaTime;

            // spin
            _angularVelocity = MathHelper.ToDegrees(Velocity.Magnitude() * LocalSize.X * 0.5f * deltaTime) * Constants.BALL_SPIN_SCALE;
        }

        public void Reflect(Vector2 normal)
        {
            Reflect(normal, Constants.BALL_BOUNCE);
        }

        public void Reflect(Vector2 normal, float scale)
        {
            normal.Normalize();

            Velocity -= 2.0f * Vector2.Dot(Velocity, normal) * normal;

            // if a tennis ball, do not lose velocity
            if (_ballType != BallType.TennisBall)
            {
                // lose some inertia from hitting the wall, but lose more if it is head on vs. skimming the wall
                Vector2 normalizedVelocity = Velocity;
                normalizedVelocity.Normalize();

                Velocity *= scale * (Constants.BALL_BOUNCE_PENALTY_PERCENT * (1.0f - MathF.Abs(Vector2.Dot(normal, normalizedVelocity))) + (1.0f - Constants.BALL_BOUNCE_PENALTY_PERCENT));
            }
        }

        #endregion

        #region Events

        private void Break()
        {
            if(_state == State.Moving)
            {
                LevelScene levelScene = (LevelScene)Scene;

                // stop the ball
                Stop();

                // set to broken state
                _state = State.Broken;

                // add a stroke
                Owner.Stroke++;
                levelScene.OnBallStroke();

                // set broken delay timer
                _maxTime = Constants.BALL_BROKEN_DELAY;
                _timer = _maxTime;

                // set the visual
                Sprite = new Sprite(Scene.Content.Load<Texture2D>("Texture/MintBroken"), null, new Vector2(0.5f));

                // scale 2x since the texture is twice as big
                LocalScale *= 2.0f;
            }
        }

        public void Sink(LevelObject hole)
        {
            if (_state == State.Moving)
            {
                // stop the ball
                Stop();

                hole.PlaySound();

                // set to sinking state
                _state = State.Sinking;

                // forced move into hole
                _start = LocalPosition;
                _end = hole.LocalPosition;

                // set the movement timer
                _maxTime = Constants.BALL_SINK_TIME;
                _timer = _maxTime;
            }
        }

        public void Die(LevelObject from)
        {
            if (_state == State.Moving)
            {
                // play sound
                from.PlaySound();

                // set to dying
                _state = State.Dying;

                // set shrink timer
                _maxTime = Constants.BALL_DEATH_TIME;
                _timer = _maxTime;
            }
        }

        private void Teleport(LevelObject from)
        {
            if (_state == State.Moving && from != _portal)
            {
                // play sound
                from.PlaySound();

                // set to teleporting
                _state = State.TeleportingIn;

                // set shrink timer
                _maxTime = Math.Min(Constants.BALL_TELEPORT_MAX_TIME, Constants.BALL_TELEPORT_VELOCITY_SCALE / Speed);
                _timer = _maxTime;

                // set target
                _portal = from;
                _start = LocalPosition;
                _end = from.LocalPosition;
            }
        }

        #endregion
    }
}
