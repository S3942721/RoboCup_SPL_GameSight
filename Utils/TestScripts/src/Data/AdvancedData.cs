using System;
using System.IO;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Data
{
    [Serializable]
    public class AdvancedData : GameControlData, ICloneable
    {
        private const long serialVersionUID = 2720243434306304319L;

        public string message = "";
        public long timeBeforeCurrentGameState;
        public long timeBeforeStoppageOfPlay;
        public long whenCurrentGameStateBegan;
        public long whenCurrentSetPlayBegan;
        public long timeSinceCurrentGameStateBegan;
        public long timeSinceCurrentSetPlayBegan;
        public readonly long[][] whenPenalized = new long[2][];
        public readonly int[] penaltyCount = new int[2];
        public readonly int[][] robotPenaltyCount = new int[2][];
        public readonly int[][] robotHardwarePenaltyBudget = new int[2][];
        public readonly bool[][] ejected = new bool[2][];
        public readonly bool[] sentIllegalMessages = { false, false };
        public readonly bool refereeTimeout = false;
        public readonly bool[] timeOutActive = { false, false };
        public bool[] timeOutTaken = { false, false };
        public bool leftSideKickoff = true;
        public bool testmode = false;
        public readonly bool manPause = false;
        public readonly bool manPlay = false;
        public long manWhenClockChanged;
        public long manTimeOffset;
        public long manRemainingGameTimeOffset;
        public readonly byte previousGamePhase = GAME_PHASE_NORMAL;
        public readonly byte kickingTeamBeforeGoal = 0;
        public static readonly byte KICKOFF_HALF = 0;
        public static readonly byte KICKOFF_TIMEOUT = 1;
        public static readonly byte KICKOFF_GAMESTUCK = 2;
        public static readonly byte KICKOFF_PENALTYSHOOT = 3;
        public static readonly byte KICKOFF_GOAL = 4;
        public byte kickOffReason = KICKOFF_HALF;
        public readonly int[][] penaltyShootOutPlayers = new int[][] { new int[] { -1, -1 }, new int[] { -1, -1 } };

        public AdvancedData()
        {
            if (Rules.league.startWithPenalty)
            {
                gamePhase = GAME_PHASE_PENALTYSHOOT;
                kickOffReason = KICKOFF_PENALTYSHOOT;
            }

            for (int i = 0; i < 2; i++)
            {
                whenPenalized[i] = new long[Rules.league.teamSize];
                robotPenaltyCount[i] = new int[Rules.league.teamSize];
                robotHardwarePenaltyBudget[i] = new int[Rules.league.teamSize];
                ejected[i] = new bool[Rules.league.teamSize];

                for (int j = 0; j < Rules.league.teamSize; j++)
                {
                    if (j >= Rules.league.robotsPlaying)
                    {
                        team[i].player[j].penalty = PlayerInfo.PENALTY_SUBSTITUTE;
                    }

                    if (j < robotHardwarePenaltyBudget[i].Length)
                    {
                        robotHardwarePenaltyBudget[i][j] = Rules.league.allowedHardwarePenaltiesPerHalf;
                    }
                }
            }
        }

        public object Clone()
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(AdvancedData));
                    serializer.WriteObject(stream, this);
                    stream.Position = 0;
                    return serializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.GetType().Name}: {e.Message}");
            }

            return null; // Should never be reached
        }

        public int GetSide(short teamNumber)
        {
            return teamNumber == team[0].teamNumber ? 0 : 1;
        }

        public long GetTime()
        {
            return manPause ? manWhenClockChanged : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + manTimeOffset;
        }

        public int getSecondsSince(long millis)
        {
            return millis == 0 ? 100000 : (int)(GetTime() - millis) / 1000;
        }

        public int GetRemainingSeconds(long millis, int durationInSeconds)
        {
            return durationInSeconds - getSecondsSince(millis);
        }

        public void UpdateTimes(bool real)
        {
            secsRemaining = (short)GetRemainingGameTime(real);
            int? subT = GetSecondaryTime(real);

            if (subT == null)
            {
                secondaryTime = 0;
            }
            else
            {
                secondaryTime = (short)(int)subT;
            }

            for (int side = 0; side < team.Length; ++side)
            {
                for (int number = 0; number < team[side].player.Length; ++number)
                {
                    PlayerInfo player = team[side].player[number];
                    player.secsTillUnpenalised = player.penalty == PlayerInfo.PENALTY_NONE
                        ? (byte)0
                        : (byte)(number < ejected[side].Length && ejected[side][number] ? 255 : Math.Min(255, GetRemainingPenaltyTime(side, number, real)));
                }
            }
        }
        public void AddTimeInCurrentState()
        {
            timeBeforeCurrentGameState += GetTime() - whenCurrentGameStateBegan;
        }

        public void AddTimeInCurrentStateToPenalties()
        {
            for (int side = 0; side < team.Length; side++)
            {
                for (int number = 0; number < whenPenalized[side].Length; number++)
                {
                    if (team[side].player[number].penalty != PlayerInfo.PENALTY_NONE && whenPenalized[side][number] != 0)
                    {
                        whenPenalized[side][number] += GetTime() - Math.Max(whenCurrentGameStateBegan, whenPenalized[side][number]);
                    }
                }
            }
        }

        public int GetRemainingGameTime(bool real)
        {
            int duration = gamePhase == GAME_PHASE_TIMEOUT
                ? (previousGamePhase == GAME_PHASE_NORMAL ? Rules.league.halfTime
                        : previousGamePhase == GAME_PHASE_OVERTIME ? Rules.league.overtimeTime
                                : Rules.league.penaltyShotTime)
                : (gamePhase == GAME_PHASE_NORMAL) ? Rules.league.halfTime
                        : gamePhase == GAME_PHASE_OVERTIME ? Rules.league.overtimeTime
                                : Math.Max(team[0].penaltyShot, team[1].penaltyShot) > Rules.league.numberOfPenaltyShots
                                ? Rules.league.penaltyShotTimeSuddenDeath
                                : Rules.league.penaltyShotTime;

            int timePlayed = gameState == STATE_INITIAL
                || ((gameState == STATE_READY || gameState == STATE_SET)
                    && (competitionPhase == COMPETITION_PHASE_PLAYOFF && Rules.league.playOffTimeStop
                        && (real || gamePhase != GAME_PHASE_NORMAL || gameState != STATE_READY || kickOffReason != KICKOFF_GOAL
                            || getSecondsSince(whenCurrentGameStateBegan) >= Rules.league.delayedSwitchAfterGoal)
                        || timeBeforeCurrentGameState == 0))
                || gameState == STATE_FINISHED
                        ? (int)((timeBeforeCurrentGameState + manRemainingGameTimeOffset + (manPlay ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - manWhenClockChanged : 0)) / 1000)
                        : real || (competitionPhase != COMPETITION_PHASE_PLAYOFF && timeBeforeCurrentGameState > 0) || gameState != STATE_PLAYING
                        || getSecondsSince(whenCurrentGameStateBegan) >= Rules.league.delayedSwitchToPlaying
                        ? getSecondsSince(whenCurrentGameStateBegan - timeBeforeCurrentGameState - manRemainingGameTimeOffset)
                        : (int)((timeBeforeCurrentGameState - manRemainingGameTimeOffset) / 1000);

            return duration - timePlayed;
        }

        public int? GetRemainingPauseTime()
        {
            if (gamePhase == GAME_PHASE_NORMAL && competitionType != COMPETITION_TYPE_DYNAMIC_BALL_HANDLING
                && (gameState == STATE_INITIAL && firstHalf != C_TRUE && !timeOutActive[0] && !timeOutActive[1]
                || gameState == STATE_FINISHED && firstHalf == C_TRUE))
            {
                return GetRemainingSeconds(whenCurrentGameStateBegan, Rules.league.pauseTime);
            }
            else if (Rules.league.pausePenaltyShootOutTime != 0 && competitionPhase == COMPETITION_PHASE_PLAYOFF && team[0].score == team[1].score
                && (gameState == STATE_INITIAL && gamePhase == GAME_PHASE_PENALTYSHOOT && !timeOutActive[0] && !timeOutActive[1]
                || gameState == STATE_FINISHED && firstHalf != C_TRUE))
            {
                return GetRemainingSeconds(whenCurrentGameStateBegan, Rules.league.pausePenaltyShootOutTime);
            }
            else
            {
                return null;
            }
        }

        // MISSING METHODS

        // resetPenaltyTimes()
        public void ResetPenaltyTimes()
        {
            foreach (long[] players in whenPenalized)
            {
                Array.Fill(players, 0);
            }
        }

        // resetPenalties()
        public void ResetPenalties()
        {
            for (int i = 0; i < team.Length; ++i)
            {
                for (int j = 0; j < Rules.league.teamSize; j++)
                {
                    if (team[i].player[j].penalty != PlayerInfo.PENALTY_SUBSTITUTE && !ejected[i][j])
                    {
                        team[i].player[j].penalty = PlayerInfo.PENALTY_NONE;
                    }

                    if (Rules.league.resetPenaltyCountOnHalftime)
                    {
                        robotPenaltyCount[i][j] = 0;
                    }

                    robotHardwarePenaltyBudget[i][j] = Math.Min(
                        Rules.league.allowedHardwarePenaltiesPerHalf,
                        Rules.league.allowedHardwarePenaltiesPerGame - (Rules.league.allowedHardwarePenaltiesPerHalf - robotHardwarePenaltyBudget[i][j]));
                }

                if (Rules.league.resetPenaltyCountOnHalftime)
                {
                    penaltyCount[i] = 0;
                }
            }

            ResetPenaltyTimes();
        }


        // getRemainingPenaltyTime()

        public int GetRemainingPenaltyTime(int side, int number, bool real)
        {
            int penalty = team[side].player[number].penalty;
            int penaltyTime = GetPenaltyDuration(side, number);

            if (penaltyTime == -1)
            {
                return 0;
            }

            Debug.Assert(penalty != PlayerInfo.PENALTY_MANUAL && penalty != PlayerInfo.PENALTY_SUBSTITUTE);

            long start = whenPenalized[side][number];

            if (start != 0 && (gameState == STATE_SET || (!real
                && gameState == STATE_PLAYING
                && getSecondsSince(whenCurrentGameStateBegan) < Rules.league.delayedSwitchToPlaying)))
            {
                start += GetTime() - Math.Max(whenCurrentGameStateBegan, whenPenalized[side][number]);
            }

            return Math.Max(0, GetRemainingSeconds(start, penaltyTime));
        }


        // getPenaltyDuration
        public int GetPenaltyDuration(int side, int number)
        {
            int penalty = team[side].player[number].penalty;
            int penaltyTime = -1;

            if (penalty != PlayerInfo.PENALTY_MANUAL && penalty != PlayerInfo.PENALTY_SUBSTITUTE)
            {
                penaltyTime = Rules.league.penaltyTime[penalty] + Rules.league.penaltyIncreaseTime * robotPenaltyCount[side][number];
            }

            Debug.Assert(penalty == PlayerInfo.PENALTY_MANUAL || penalty == PlayerInfo.PENALTY_SUBSTITUTE || penaltyTime != -1);

            return penaltyTime;
        }


        // getNumberOfRobotsInPlay()
        public int GetNumberOfRobotsInPlay(int side)
        {
            int count = 0;

            for (int i = 0; i < team[side].player.Length; i++)
            {
                if (team[side].player[i].penalty != PlayerInfo.PENALTY_SUBSTITUTE)
                {
                    count++;
                }
            }

            return count;
        }


        // getSecondaryTime()

        public int? GetSecondaryTime(bool real)
        {
            if (!real && (gameState == STATE_PLAYING
                          && getSecondsSince(whenCurrentGameStateBegan) < Rules.league.delayedSwitchToPlaying
                          || gamePhase == GAME_PHASE_NORMAL && gameState == STATE_READY
                          && kickOffReason == KICKOFF_GOAL
                          && getSecondsSince(whenCurrentGameStateBegan) < Rules.league.delayedSwitchAfterGoal))
            {
                return null;
            }

            int timeKickOffBlocked = GetRemainingSeconds(whenCurrentGameStateBegan, Rules.league.kickoffTime);

            if (gameState == STATE_INITIAL && (timeOutActive[0] || timeOutActive[1]))
            {
                return GetRemainingSeconds(whenCurrentGameStateBegan, Rules.league.timeOutTime);
            }
            else if (gameState == STATE_INITIAL && (refereeTimeout))
            {
                return GetRemainingSeconds(whenCurrentGameStateBegan, Rules.league.refereeTimeout);
            }
            else if (gameState == STATE_READY)
            {
                return GetRemainingSeconds(whenCurrentGameStateBegan,
                    setPlay == SET_PLAY_PENALTY_KICK
                        ? Rules.league.penaltyKickReadyTime
                        : Rules.league.readyTime);
            }
            else if (gameState == STATE_PLAYING && gamePhase != GAME_PHASE_PENALTYSHOOT
                     && (setPlay == SET_PLAY_GOAL_KICK || setPlay == SET_PLAY_PUSHING_FREE_KICK
                         || setPlay == SET_PLAY_CORNER_KICK || setPlay == SET_PLAY_KICK_IN))
            {
                return GetRemainingSeconds(whenCurrentSetPlayBegan, Rules.league.freeKickTime);
            }
            else if (gameState == STATE_PLAYING && gamePhase != GAME_PHASE_PENALTYSHOOT
                     && setPlay == SET_PLAY_PENALTY_KICK)
            {
                return GetRemainingSeconds(whenCurrentGameStateBegan, Rules.league.penaltyShotTime);
            }
            else if (gameState == STATE_PLAYING && kickOffReason != KICKOFF_PENALTYSHOOT
                     && timeKickOffBlocked >= 0)
            {
                return timeKickOffBlocked;
            }
            else
            {
                return GetRemainingPauseTime();
            }
        }




        public void UpdatePenalties()
        {
            if (gamePhase == GAME_PHASE_NORMAL && gameState == STATE_PLAYING
                && getSecondsSince(whenCurrentGameStateBegan) >= Rules.league.delayedSwitchToPlaying
                && Rules.league is SPL)
            {
                foreach (TeamInfo t in team)
                {
                    foreach (PlayerInfo p in t.player)
                    {
                        if (p.penalty == PlayerInfo.PENALTY_SPL_ILLEGAL_MOTION_IN_SET)
                        {
                            p.penalty = PlayerInfo.PENALTY_NONE;
                        }
                    }
                }
            }
        }

        public void AdjustTimestamps(long originalTime)
        {
            long timeUpdate = GetTime() - originalTime;
            if (whenCurrentGameStateBegan != 0)
            {
                whenCurrentGameStateBegan += timeUpdate;
            }
            if (whenCurrentSetPlayBegan != 0)
            {
                whenCurrentSetPlayBegan += timeUpdate;
            }
            for (int i = 0; i < whenPenalized.Length; ++i)
            {
                for (int j = 0; j < whenPenalized[i].Length; ++j)
                {
                    if (whenPenalized[i][j] != 0)
                    {
                        whenPenalized[i][j] += timeUpdate;
                    }
                }
            }
            if (manWhenClockChanged != 0)
            {
                manWhenClockChanged += timeUpdate;
            }
        }
    }

}
