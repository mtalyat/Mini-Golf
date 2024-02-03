using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
    internal class ObjectTypeData
    {
        private readonly ObjectType _objectType;
        public ObjectType Type => _objectType;

        private readonly Rectangle? _rectangle;
        public Rectangle? Rect => _rectangle;

        private readonly Vector2? _pivot;
        public Vector2? Pivot => _pivot;

        private readonly float? _depth;
        public float? Depth => _depth;

        private readonly string _soundEffect;
        public string SoundEffect => _soundEffect;

        public ObjectTypeData(ObjectType objectType, Rectangle? rectangle, Vector2? pivot, float? depth, string soundEffect)
        {
            _objectType = objectType;
            _rectangle = rectangle;
            _pivot = pivot;
            _depth = depth;
            _soundEffect = soundEffect;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectTypeData data &&
                   _objectType == data._objectType &&
                   _rectangle.Equals(data._rectangle) &&
                   _pivot.Equals(data._pivot) &&
                   _depth == data._depth &&
                   _soundEffect.Equals(data._soundEffect);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_objectType, _rectangle, _pivot, _depth, _soundEffect);
        }

        public static ObjectTypeData FromString(string str)
        {
            // split by separating tab
            string[] parts = str.Split(": ");

            string[] subParts = parts[1].Split('/');

            // get data for the object
            ObjectType type = Enum.Parse<ObjectType>(parts[0]);
            Rectangle? rect = subParts.Length <= 0 ? null : Parse.ParseRectangle(subParts[0]);
            Vector2? pivot = subParts.Length <= 1 ? null : Parse.ParseVector2(subParts[1]);
            float? depth = subParts.Length <= 2 ? null : float.Parse(subParts[2]);
            string soundEffect = subParts.Length <= 3 || subParts.Length == 0 ? null : subParts[3];

            return new ObjectTypeData(type, rect, pivot, depth, soundEffect);
        }

        public override string ToString()
        {
            Rectangle rect = Rect ?? Rectangle.Empty;
            Vector2 pivot = Pivot ?? Vector2.Zero;
            float depth = Depth ?? 0.0f;

            return $"{Type}\t{rect.X} {rect.Y} {rect.Width} {rect.Height}/{pivot.X} {pivot.Y}/{depth}/{SoundEffect}";
        }
    }
}
