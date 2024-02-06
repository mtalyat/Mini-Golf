using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal static class Constants
    {
        public const string APPLICATION_NAME_SAFE = "Mini Golf";
        public const string APPLICATION_NAME_UNSAFE = "Mini Golf?";

        public const string BUILTIN_WORLD_NAME = "Builtin";

        public const int RESOLUTION_WIDTH = 1920;
        public const int RESOLUTION_HEIGHT = 1080;
        public static Vector2 RESOLUTION => new(RESOLUTION_WIDTH, RESOLUTION_HEIGHT);
        public static Vector2 RESOLUTION_HALF => new(RESOLUTION_WIDTH >> 1, RESOLUTION_HEIGHT >> 1);

        public const float CAMERA_ZOOM_FACTOR = 2.0f;
        public const float CAMERA_ZOOM_SPEED = 10.0f;
        public const float CAMERA_LERP_SPEED = 5.0f;
        public const float CAMERA_SPACING_X = RESOLUTION_WIDTH * 0.5f;
        public const float CAMERA_SPACING_Y = RESOLUTION_HEIGHT* 0.5f;

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
        // the velocity magnitude where the time will be 1 second to go in/1 second to go out of the portal
        public const float BALL_TELEPORT_VELOCITY_SCALE = 200.0f;
        public const float BALL_TELEPORT_MAX_TIME = 2.0f;
        public const float BALL_TELEPORT_SPIN_AMOUNT = MathF.PI * 2.0f;
        public const float BALL_BROKEN_DELAY = 1.0f;

        public const float LEVEL_DAMAGED_WALL_SLOW_DOWN = 0.25f;
        public const float LEVEL_BOX_SLOW_DOWN = 0.33f;
        public const float LEVEL_SMASHED_TIME = 3.0f;
        public const float LEVEL_SLOPE_FORCE = 400.0f;
        public const float LEVEL_SANDBAR_FORCE = 6.0f;
        public const float LEVEL_PAUSE_TIME = 2.0f;
        public const float LEVEL_BOUNCE_POWER = 250.0f;
        public const float LEVEL_BOUNCE_SCALE = 1.2f;
        public const float LEVEL_BOUNCE_TIME = 0.2f;
        public const float LEVEL_SPIN_SPEED = 90.0f;

        public const float EDITOR_EDIT_COOLDOWN_TIME_INITIAL = 0.5f;
        public const float EDITOR_EDIT_COOLDOWN_TIME_REPEAT = 0.025f;

        public const float PREVIEW_ANIMATION_SPEED = 50.0f;

        // local path to built in worlds
        public static string PATH_BUILTIN => Path.Combine("Content", "Level");
        // global path to custom levels (in "Documents/My Games" folder on Windows)
        public static string PATH_CUSTOM => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", APPLICATION_NAME_SAFE);

        public static string PATH_LEVEL_EXTENSION = ".level";
        public static string PATH_INFO_EXTENSION = ".txt";

        public static readonly Rectangle UI_BUTTON = new(0, 0, 320, 160);
        public static readonly Rectangle UI_PANEL = new(0, 160, 320, 320);
        public static readonly Rectangle UI_BACKGROUND = new(320, 0, 160, 160);
    }
}
