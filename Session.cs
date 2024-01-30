using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    /// <summary>
    /// Holds data for the current play session.
    /// </summary>
    internal static class Session
    {
        public static List<Player> Players { get; set; } = new();
    }
}
