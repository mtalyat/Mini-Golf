﻿using Microsoft.Xna.Framework;
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
        public bool IsMoving => Velocity.X != 0.0f && Velocity.Y != 0.0f;

        private float _angularVelocity = 0.0f;
        private bool _shouldSpin = false;

        public float Radius => GetGlobalSize().X / 2.0f; // should be square, so just use the x axis

        private TrailObject _trail = null;
        private AimingObject _aiming = null;

        public bool Dead => _state == State.Dead;
        public bool Sunk => _state == State.Sunk;
        private float _timer;
        private float _maxTime;
        private Vector2 _start;
        private Vector2 _end;

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
            switch(type)
            {
                case BallType.SoccerBall:
                    _objectFriction = Constants.BALL_FRICTION * 2.0f;
                    break;
                default:
                    _objectFriction = Constants.BALL_FRICTION;
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // if no longer moving and was moving, mark as done
            if(_state == State.Moving)
            {
                if(IsMoving)
                {
                    if(_shouldSpin)
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
                else
                {
                    // no longer moving
                    _state = State.Done;
                }
            }

            // if not moving and self was clicked, spawn a trail
            if (_state == State.Idle)
            {
                ButtonState state = Input.GetMouseButtonState(0);
                if (state == ButtonState.Down && Input.ContainsMouse(GetHitbox()))
                {
                    // spawn a trail to the mouse
                    _trail = Scene.Instantiate(new TrailObject(GetGlobalCenter(), GetGlobalSize().X, Scene));

                    // if a pool ball, spawn an aiming object
                    _aiming = Scene.Instantiate(new AimingObject(GetGlobalCenter(), Scene));

                    // set depth to render right under the ball
                    _trail.Depth = Depth - 0.0001f;
                    _aiming.Depth = Depth - 0.0001f;
                }
                else if (_trail != null)
                {
                    if (state == ButtonState.Pressed)
                    {
                        // if button held, make ball face the angle of the trail
                        LocalRotation = MathHelper.ToDegrees(GetTrailAngle());
                    }
                    else if(state == ButtonState.Up)
                    {
                        // if button released, hit and destroy the trail
                        Hit(Vector2Helper.FromAngle(GetTrailAngle()), _trail.GetGlobalSize().Y);
                        _trail.Destroy();
                        _trail = null;
                        _aiming?.Destroy();
                        _aiming = null;
                    }
                }
            }

            if(_state == State.Sinking || _state == State.Dying)
            {
                // shrinking
                _timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // update scale
                float percent = MathF.Max(_timer, 0.0f) / _maxTime;
                float scale = percent;
                LocalScale = new Vector2(scale, scale);

                // update position
                if(_state == State.Sinking)
                {
                    LocalPosition = Vector2Helper.Lerp(_end, _start, percent);
                }

                // if timer is done, finish
                if(_timer <= 0.0f)
                {
                    if(_state == State.Sinking)
                    {
                        _state = State.Sunk;
                    } else if (_state == State.Dying)
                    {
                        _state = State.Dead;
                    }
                    else
                    {
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

        #region Trail

        private float GetTrailAngle()
        {
            return MathHelper.ToRadians(_trail.GetGlobalRotation()) - MathF.PI / 2.0f;
        }

        #endregion

        #region Collisions

        public bool PreCollideWith(LevelObject obj)
        {
            switch(obj.Type)
            {
                case ObjectType.WallDamaged:
                    // break if heavy or if a football that is not spinning
                    if (_weight == Weight.Heavy || (_ballType == BallType.FootballBall && !_shouldSpin))
                    {
                        obj.Destroy();

                        // if not football, slow down slightly
                        if (BallType != BallType.FootballBall)
                        {
                            SlowDown(Constants.LEVEL_DAMAGED_WALL_SLOW_DOWN);
                        }

                        return false;
                    }
                    break;
                case ObjectType.Box:
                    // kill box if heavy
                    if (_weight == Weight.Heavy)
                    {
                        obj.Destroy();

                        // slow down slightly
                        SlowDown(Constants.LEVEL_BOX_SLOW_DOWN);

                        return false;
                    }
                    break;
            }

            return true;
        }

        public void CollideWith(LevelObject obj, float deltaTime)
        {
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
                    // spin if hit a wall
                    _shouldSpin = true;
                    break;
                case ObjectType.Hole:
                    // if the ball is over the hole, sink into it
                    if (obj.GetHitbox().ContainsRound(GetGlobalCenter()))
                    {
                        Sink(obj);
                    }
                    break;
                case ObjectType.Slope:
                    // get vector direction of slope from its angle, add to velocity
                    Push(Vector2Helper.FromAngle(MathHelper.ToRadians(obj.GetGlobalRotation())) * Constants.LEVEL_SLOPE_FORCE * deltaTime);
                    break;
                case ObjectType.Hill:
                    // get vector direction of slope from center of the hill, add to velocity
                    Push(Vector2Helper.FromAngle(Vector2Helper.Angle(obj.GetGlobalCenter(), GetGlobalCenter())) * Constants.LEVEL_SLOPE_FORCE * deltaTime);
                    break;
                case ObjectType.Valley:
                    // get vector direction of slope from center of the hill, subtract from velocity
                    Push(-Vector2Helper.FromAngle(Vector2Helper.Angle(obj.GetGlobalCenter(), GetGlobalCenter())) * Constants.LEVEL_SLOPE_FORCE * deltaTime);
                    break;
                case ObjectType.Sandbar:
                    // slow down the ball
                    Velocity *= 1.0f - Math.Min(Constants.LEVEL_SANDBAR_FORCE * deltaTime, 1.0f);
                    break;
                case ObjectType.Water:
                    // kill ball if center is in the hazard
                    if (obj.GetHitbox().Contains(GetGlobalCenter()))
                    {
                        Die();
                    }
                    break;
            }
        }

        #endregion

        #region Movement

        public void Hit(Vector2 directionAndPower)
        {
            // move the ball
            Velocity = Constants.BALL_HIT_POWER * (2.0f - (float)_weight / 2.0f) * directionAndPower;
            // mark as moving
            _state = State.Moving;
            // mark as spinning if not a football
            _shouldSpin = _ballType != BallType.FootballBall;
            // add a stroke
            _owner.Stroke++;
        }

        public void Hit(Vector2 direction, float power)
        {
            Hit(direction * power);
        }

        public void Push(Vector2 directionAndPower)
        {
            // soccer ball not affected by pushing
            if(_ballType != BallType.SoccerBall)
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
            if(_state != State.Moving)
            {
                return;
            }

            // slow from friction
            SlowDown(_objectFriction * deltaTime);

            // if velocity is < 1, stop
            // < 1 indicates that it is moving less than one pixel a second
            if (Velocity.Magnitude() < Constants.BALL_STOP_THRESHOLD)
            {
                Stop();
            }

            // move
            LocalPosition += Velocity * deltaTime;

            // spin
            _angularVelocity = MathHelper.ToDegrees(Velocity.Magnitude() * Radius * deltaTime) * Constants.BALL_SPIN_SCALE;
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

        public void Sink(LevelObject hole)
        {
            if(_state == State.Moving)
            {
                Stop();
                _start = GetGlobalCenter();
                _end = hole.GetGlobalCenter();
                _state = State.Sinking;
                _maxTime = Constants.BALL_SINK_TIME;
                _timer = _maxTime;
            }
        }

        public void Die()
        {
            if (_state == State.Moving)
            {
                //Stop();
                _state = State.Dying;
                _maxTime = Constants.BALL_DEATH_TIME;
                _timer = _maxTime;
            }
        }

        #endregion
    }
}
