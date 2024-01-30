using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal class Player
    {
        public string Name { get; set; }

        public Color Color { get; set; }

        public int Stroke { get; set; } = 0;

        public Player(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
}
