using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniGolf.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class BallObject : LevelObject
    {
        public enum Weight
        {
            Light = 1,
            Normal = 2,
            Heavy = 3,
        }

        private readonly Player _owner;
        public Player Owner => _owner;

        private Weight _weight;
        public Weight WeightClass => _weight;

        private readonly BallType _ballType;
        public BallType BallType => _ballType;

        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public bool IsMoving => Velocity.X != 0.0f && Velocity.Y != 0.0f;

        public float Radius => GetGlobalSize().X / 2.0f; // should be square, so just use the x axis

        private float _objectFriction;

        private TrailObject _trail = null;

        public BallObject(BallType type, Player owner, Scene scene) : base(ObjectType.Ball, new Sprite(scene.Content.Load<Texture2D>($"Texture/{type}")), scene)
        {
            _owner = owner;
            LocalSize = new Vector2(20.0f, 20.0f);
            Depth = 1.0f;

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
            // if not moving and self was clicked, spawn a trail
            ButtonState state = Input.GetMouseButtonState(0);
            if (!IsMoving)
            {
                if (state == ButtonState.Down && Input.ContainsMouse(GetHitbox()))
                {
                    _trail = Scene.Instantiate(new TrailObject(GetGlobalCenter(), GetGlobalSize().X, Scene));

                    // set depth to render right under the ball
                    _trail.Depth = Depth - 0.0001f;
                }
            }
            
            // if button released, hit and destroy the trail
            if (state == ButtonState.Up && _trail != null)
            {
                Hit(Vector2Helper.FromAngle(MathHelper.ToRadians(_trail.GetGlobalRotation()) - MathF.PI / 2.0f), _trail.GetGlobalSize().Y);
                _trail.Destroy();
                _trail = null;
            }

            base.Update(gameTime);
        }

        public void Hit(Vector2 directionAndPower)
        {
            Velocity = Constants.BALL_HIT_POWER * (2.0f - (float)_weight / 2.0f) * directionAndPower;
        }

        public void Hit(Vector2 direction, float power)
        {
            Hit(direction * power);
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
        }
    }
}
