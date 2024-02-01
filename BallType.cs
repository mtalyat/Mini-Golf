using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniGolf
{
    internal enum BallType : int
    {
        GolfBall,

        TennisBall,

        FootballBall,

        BowlingBall,

        PongBall,

        SoccerBall,

        PoolBall,

        HockeyPuck,

        Mint
    }

    internal static class BallTypeExtensions
    {
        public const int BALL_TYPE_COUNT = 9;
    }
}
