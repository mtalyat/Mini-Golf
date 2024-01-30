using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal static class Constants
    {
        public const int RESOLUTION_WIDTH = 1920;
        public const int RESOLUTION_HEIGHT = 1080;

        public const string CONTENT_ROOT_DIRECTORY = "Content";

        public const float BALL_HIT_POWER = 2.0f;
        public const float BALL_FRICTION = 0.5f;
        public const float BALL_STOP_THRESHOLD = 5.0f;

        public const float LEVEL_DAMAGED_WALL_SLOW_DOWN = 0.1f;
        public const float LEVEL_SLOPE_FORCE = 1.0f;
        public const float LEVEL_SANDBAR_FORCE = 100.0f;
    }
}
