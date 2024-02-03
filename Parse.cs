using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal static class Parse
    {
        public static Vector2 ParseVector2(string str)
        {
            if(string.IsNullOrEmpty(str)) return Vector2.Zero;

            string[] split = str.Split(' ');
            return new Vector2(ParseFloat(split[0]), ParseFloat(split[1]));
        }

        public static Vector3 ParseVector3(string str)
        {
            if (string.IsNullOrEmpty(str)) return Vector3.Zero;

            string[] split = str.Split(' ');
            return new Vector3(ParseFloat(split[0]), ParseFloat(split[1]), ParseFloat(split[2]));
        }

        public static Color ParseColor(string str)
        {
            if (string.IsNullOrEmpty(str)) return new Color();

            string[] split = str.Split(' ');
            return new Color(ParseInt(split[0]), ParseInt(split[1]), ParseInt(split[2]), split.Length < 4 ? 255 : ParseInt(split[3]));
        }

        public static Rectangle ParseRectangle(string str)
        {
            if (string.IsNullOrEmpty(str)) return Rectangle.Empty;

            string[] split = str.Split(' ');
            return new Rectangle(
                ParseInt(split[0]),
                ParseInt(split[1]),
                ParseInt(split[2]),
                ParseInt(split[3])
                );
        }

        public static int ParseInt(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;

            return int.Parse(str);
        }

        public static float ParseFloat(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0.0f;

            return float.Parse(str);
        }
    }
}
