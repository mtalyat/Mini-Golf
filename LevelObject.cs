using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MiniGolf
{
    internal class LevelObject : SpriteObject
    {
        private readonly ObjectType _type;
        public ObjectType Type => _type;

        private readonly BehaviorFlags _behaviorFlags;
        public BehaviorFlags Flags => _behaviorFlags;

        public LevelObject(ObjectType type, Sprite sprite, Scene scene) : base(sprite, scene)
        {
            _type = type;
            _behaviorFlags = BehaviorFlags.None;

            // determine flags from type
            switch (type)
            {
                case ObjectType.Generic:
                    break;
                case ObjectType.Ball:
                    _behaviorFlags = BehaviorFlags.Collidable | BehaviorFlags.Solid | BehaviorFlags.Round;
                    break;
                case ObjectType.Hole:
                    _behaviorFlags = BehaviorFlags.Collidable | BehaviorFlags.Round;
                    break;
                case ObjectType.Start:
                    break;
                case ObjectType.WallDamaged:
                    _behaviorFlags = BehaviorFlags.Static | BehaviorFlags.Collidable | BehaviorFlags.Solid | BehaviorFlags.Breakable;
                    break;
                case ObjectType.Wall:
                case ObjectType.Wall1:
                case ObjectType.Wall2:
                case ObjectType.Wall3:
                case ObjectType.Wall4:
                    _behaviorFlags = BehaviorFlags.Static | BehaviorFlags.Collidable | BehaviorFlags.Solid;
                    break;
                case ObjectType.Slope:
                    _behaviorFlags = BehaviorFlags.Static | BehaviorFlags.Collidable;
                    break;
                case ObjectType.Hill:
                case ObjectType.Valley:
                case ObjectType.Sand:
                    _behaviorFlags = BehaviorFlags.Static | BehaviorFlags.Collidable | BehaviorFlags.Round;
                    break;
                case ObjectType.Water:
                    _behaviorFlags = BehaviorFlags.Static | BehaviorFlags.Collidable | BehaviorFlags.Hazzard;
                    break;
                case ObjectType.Box:
                    _behaviorFlags = BehaviorFlags.Collidable | BehaviorFlags.Solid | BehaviorFlags.Breakable;
                    break;
                case ObjectType.Crate:
                    _behaviorFlags = BehaviorFlags.Collidable | BehaviorFlags.Solid;
                    break;
                //case ObjectType.RotatingWall:
                //case ObjectType.MovingWall:
                //    _behaviorFlags = BehaviorFlags.Collidable | BehaviorFlags.Solid;
                    //break;
            }
        }
    }
}
