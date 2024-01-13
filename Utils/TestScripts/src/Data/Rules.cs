using System;
using System.Drawing;

namespace Data
{
    public abstract class Rules
    {
        public static readonly Rules[] Leagues = {
            new SPL(),
            new SPLPenaltyShootout(),
            new SPL7v7(),
            new SPLDynamicBallHandling()
        };

        public static Rules GetLeagueRules(Type c)
        {
            foreach (Rules r in Leagues)
            {
                if (c.IsInstanceOfType(r) && r.GetType().IsAssignableFrom(c))
                {
                    return r;
                }
            }
            return null;
        }

        public static Rules league = Leagues[0];
        public string leagueName = ""; // Assign some default value
        public string leagueDirectory = ""; // Assign some default value
        public int teamSize = 0; // Assign some default value
        public int robotsPlaying = 0; // Assign some default value
        public Color[] teamColor = Array.Empty<Color>(); // Assign some default value
        public string[] teamColorName = Array.Empty<string>(); // Assign some default value
        public bool playOffTimeStop = false; // Assign some default value
        public int halfTime = 0; // Assign some default value
        public int readyTime = 0; // Assign some default value
        public int pauseTime = 0; // Assign some default value
        public bool kickoffChoice = false; // Assign some default value
        public int kickoffTime = 0; // Assign some default value
        public int freeKickTime = 0; // Assign some default value
        public int penaltyKickReadyTime = 0; // Assign some default value
        public int minDurationBeforeStuck = 0; // Assign some default value
        public int delayedSwitchToPlaying = 0; // Assign some default value
        public int delayedSwitchAfterGoal = 0; // Assign some default value
        public bool overtime = false; // Assign some default value
        public int overtimeTime = 0; // Assign some default value
        public bool startWithPenalty = false; // Assign some default value
        public int pausePenaltyShootOutTime = 0; // Assign some default value
        public int penaltyShotTime = 0; // Assign some default value
        public bool penaltyShotRetries = false; // Assign some default value
        public bool suddenDeath = false; // Assign some default value
        public int penaltyShotTimeSuddenDeath = 0; // Assign some default value
        public int numberOfPenaltyShots = 0; // Assign some default value
        public int[] penaltyTime = Array.Empty<int>(); // Assign some default value
        public int penaltyIncreaseTime = 0; // Assign some default value
        public bool resetPenaltyCountOnHalftime = false; // Assign some default value
        public bool allowEarlyPenaltyRemoval = false; // Assign some default value
        public byte substitutePenalty = 0; // Assign some default value
        public bool returnRobotsInGameStoppages = false; // Assign some default value
        public int timeOutTime = 0; // Assign some default value
        public int refereeTimeout = 0; // Assign some default value
        public bool isRefereeTimeoutAvailable = false; // Assign some default value
        public bool timeOutPerHalf = false; // Assign some default value
        public bool lostTime = false; // Assign some default value
        public bool dropBroadcastMessages = false; // Assign some default value
        public byte competitionType = 0; // Assign some default value
        public int allowedHardwarePenaltiesPerHalf = 0; // Assign some default value
        public int allowedHardwarePenaltiesPerGame = 0; // Assign some default value
        public short overallMessageBudget = 0; // Assign some default value
        public short additionalMessageBudgetPerMinute = 0; // Assign some default value
    }

    // public class SPL : Rules { }

    public class SPLPenaltyShootout : Rules { }

    public class SPL7v7 : Rules { }

    public class SPLDynamicBallHandling : Rules { }
}
