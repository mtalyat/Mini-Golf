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

        private readonly string _soundEffect;
        public string SoundEffect => _soundEffect;

        public ObjectTypeData(ObjectType objectType, Rectangle? rectangle, Vector2? pivot, string soundEffect)
        {
            _objectType = objectType;
            _rectangle = rectangle;
            _pivot = pivot;
            _soundEffect = soundEffect;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectTypeData data &&
                   _objectType == data._objectType &&
                   _rectangle.Equals(data._rectangle) &&
                   _pivot.Equals(data._pivot) &&
                   _soundEffect.Equals(data._soundEffect);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_objectType, _rectangle, _pivot, _soundEffect);
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
            string soundEffect = subParts.Length <= 2 || subParts.Length == 0 ? null : subParts[2];

            return new ObjectTypeData(type, rect, pivot, soundEffect);
        }

        public override string ToString()
        {
            Rectangle rect = Rect ?? Rectangle.Empty;
            Vector2 pivot = Pivot ?? Vector2.Zero;

            return $"{Type}\t{rect.X} {rect.Y} {rect.Width} {rect.Height}/{pivot.X} {pivot.Y}";
        }
    }
}
