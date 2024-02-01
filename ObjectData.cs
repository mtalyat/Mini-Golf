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
    /// <summary>
    /// Holds data for an object located within a scene.
    /// </summary>
    internal class ObjectData
    {
        /// <summary>
        /// The starting position of the object.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// The starting size of the object.
        /// </summary>
        public Vector2 Size { get; private set; }

        /// <summary>
        /// The starting rotation of the object.
        /// </summary>
        public float Rotation { get; private set; }

        /// <summary>
        /// Extra data that can be interpreted based on the type.
        /// </summary>
        public string Data { get; private set; }

        public ObjectData(Vector2 position, Vector2 size, float rotation, string data = "")
        {
            Position = position;
            Size = size;
            Rotation = rotation;
            Data = data;
        }

        public static ObjectData FromString(string line)
        {
            string[] parts = line.Split('/');

            // parse the data from the parts
            Vector2 position = parts.Length <= 0 ? Vector2.Zero : Parse.ParseVector2(parts[0]);
            Vector2 size = parts.Length <= 1 ? Vector2.Zero : Parse.ParseVector2(parts[1]);
            float rotation = parts.Length <= 2 ? 0.0f : Parse.ParseFloat(parts[2]);
            string data = parts.Length <= 3 ? string.Empty : string.Join('/', parts[3..]);

            return new ObjectData(position, size, rotation, data);
        }
    }
}
