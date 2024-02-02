using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal static class Constants
    {
        public const string APPLICATION_NAME = "Mini Golf";

        public const string BUILTIN_WORLD_NAME = "Builtin";

        public const int RESOLUTION_WIDTH = 1920;
        public const int RESOLUTION_HEIGHT = 1080;

        public const float CAMERA_ZOOM_FACTOR = 2.0f;
        public const float CAMERA_ZOOM_SPEED = 10.0f;
        public const float CAMERA_LERP_SPEED = 5.0f;

        public const string CONTENT_ROOT_DIRECTORY = "Content";

        public const float BALL_HIT_POWER = 2.0f;
        public const float BALL_FRICTION = 0.5f;
        public const float BALL_BOUNCE = 0.8f;
        public const float BALL_BOUNCE_PENALTY_PERCENT = 0.25f;
        public const float BALL_STOP_THRESHOLD = 14.0f;
        public const float BALL_STOP_TIME = 0.1f;
        public const float BALL_SINK_THRESHOLD = 400.0f;
        public const float BALL_SINK_TIME = 0.5f;
        public const float BALL_DEATH_TIME = 0.5f;
        public const float BALL_SPIN_SCALE = 0.1f;
        public const int BALL_THWACK_ITERATIONS = 1000;
        public const float BALL_THWACK_TIME = 0.2f;
        public const float BALL_MAX_POWER = RESOLUTION_WIDTH;

        public const float LEVEL_DAMAGED_WALL_SLOW_DOWN = 0.25f;
        public const float LEVEL_BOX_SLOW_DOWN = 0.33f;
        public const float LEVEL_SLOPE_FORCE = 400.0f;
        public const float LEVEL_SANDBAR_FORCE = 6.0f;
        public const float LEVEL_PAUSE_TIME = 2.0f;

        public const float PREVIEW_ANIMATION_SPEED = 50.0f;
    }
}
