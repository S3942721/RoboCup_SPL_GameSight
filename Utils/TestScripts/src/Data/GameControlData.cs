using System;
using System.IO;
using System.Text;

namespace Data
{
    [Serializable]
    public class GameControlData
    {
        public const int GAMECONTROLLER_GAMEDATA_PORT = 3838;
        public const string GAMECONTROLLER_STRUCT_HEADER = "RGme";
        public const string GAMECONTROLLER_TRUEGAMEDATA_STRUCT_HEADER = "RGTD";
        public const byte GAMECONTROLLER_STRUCT_VERSION = 15;
        public const byte TEAM_BLUE = 0;
        public const byte TEAM_RED = 1;
        public const byte TEAM_YELLOW = 2;
        public const byte TEAM_BLACK = 3;
        public const byte TEAM_WHITE = 4;
        public const byte TEAM_GREEN = 5;
        public const byte TEAM_ORANGE = 6;
        public const byte TEAM_PURPLE = 7;
        public const byte TEAM_BROWN = 8;
        public const byte TEAM_GRAY = 9;

        public const byte COMPETITION_PHASE_ROUNDROBIN = 0;
        public const byte COMPETITION_PHASE_PLAYOFF = 1;

        public const byte COMPETITION_TYPE_NORMAL = 0;
        public const byte COMPETITION_TYPE_DYNAMIC_BALL_HANDLING = 1;

        public const byte GAME_PHASE_NORMAL = 0;
        public const byte GAME_PHASE_PENALTYSHOOT = 1;
        public const byte GAME_PHASE_OVERTIME = 2;
        public const byte GAME_PHASE_TIMEOUT = 3;

        public const byte STATE_INITIAL = 0;
        public const byte STATE_READY = 1;
        public const byte STATE_SET = 2;
        public const byte STATE_PLAYING = 3;
        public const byte STATE_FINISHED = 4;

        public const byte SET_PLAY_NONE = 0;
        public const byte SET_PLAY_GOAL_KICK = 1;
        public const byte SET_PLAY_PUSHING_FREE_KICK = 2;
        public const byte SET_PLAY_CORNER_KICK = 3;
        public const byte SET_PLAY_KICK_IN = 4;
        public const byte SET_PLAY_PENALTY_KICK = 5;

        public const byte C_FALSE = 0;
        public const byte C_TRUE = 1;

        public const int SIZE = 4 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 2 + 2 + 2 * TeamInfo.SIZE;

        public bool isTrueData;

        public byte packetNumber = 0;
        public byte playersPerTeam = (byte)Rules.league.teamSize;
        public byte competitionPhase = COMPETITION_PHASE_ROUNDROBIN;
        public byte competitionType = COMPETITION_TYPE_NORMAL;
        public byte gamePhase = GAME_PHASE_NORMAL;
        public byte gameState = STATE_INITIAL;
        public byte setPlay = SET_PLAY_NONE;
        public byte firstHalf = C_TRUE;
        public byte kickingTeam;
        public short secsRemaining = (short)Rules.league.halfTime;
        public short secondaryTime = 0;
        public readonly TeamInfo[] team = new TeamInfo[2];

        public GameControlData()
        {
            for (int i = 0; i < team.Length; i++)
            {
                team[i] = new TeamInfo();
            }
            team[0].fieldPlayerColor = team[0].goalkeeperColor = TEAM_BLUE;
            team[1].fieldPlayerColor = team[1].goalkeeperColor = TEAM_RED;
        }

        public MemoryStream ToByteArray()
        {
            AdvancedData data = (AdvancedData)this;
            MemoryStream stream = new MemoryStream(SIZE);
            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);

            writer.Write(Encoding.ASCII.GetBytes(GAMECONTROLLER_STRUCT_HEADER), 0, 4);
            writer.Write(GAMECONTROLLER_STRUCT_VERSION);
            writer.Write(packetNumber);
            writer.Write(playersPerTeam);
            writer.Write(competitionPhase);
            writer.Write(competitionType);
            writer.Write(gamePhase);

            if (gameState == STATE_PLAYING && data.getSecondsSince(data.whenCurrentGameStateBegan) < Rules.league.delayedSwitchToPlaying)
            {
                writer.Write(STATE_SET);
            }
            else if (gamePhase == GAME_PHASE_NORMAL && gameState == STATE_READY && data.kickOffReason == AdvancedData.KICKOFF_GOAL
                && data.getSecondsSince(data.whenCurrentGameStateBegan) < Rules.league.delayedSwitchAfterGoal)
            {
                writer.Write(STATE_PLAYING);
            }
            else
            {
                writer.Write(gameState);
            }

            writer.Write(setPlay);
            writer.Write(firstHalf);

            if (gamePhase == GAME_PHASE_NORMAL && gameState == STATE_READY && data.kickOffReason == AdvancedData.KICKOFF_GOAL
                && data.getSecondsSince(data.whenCurrentGameStateBegan) < Rules.league.delayedSwitchAfterGoal)
            {
                writer.Write(data.kickingTeamBeforeGoal);
            }
            else
            {
                writer.Write(kickingTeam);
            }

            writer.Write(secsRemaining);
            writer.Write(secondaryTime);

            foreach (TeamInfo aTeam in team)
            {
                writer.Write(aTeam.ToByteArray(gamePhase == GAME_PHASE_NORMAL && gameState == STATE_READY
                    && data.kickOffReason == AdvancedData.KICKOFF_GOAL
                    && data.getSecondsSince(data.whenCurrentGameStateBegan) < Rules.league.delayedSwitchAfterGoal
                    && data.kickingTeam != aTeam.teamNumber));
            }

            return stream;
        }

        public MemoryStream GetTrueDataAsByteArray()
        {
            MemoryStream stream = new MemoryStream(SIZE);
            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);

            writer.Write(Encoding.ASCII.GetBytes(GAMECONTROLLER_TRUEGAMEDATA_STRUCT_HEADER), 0, 4);
            writer.Write(GAMECONTROLLER_STRUCT_VERSION);
            writer.Write(packetNumber);
            writer.Write(playersPerTeam);
            writer.Write(competitionPhase);
            writer.Write(competitionType);
            writer.Write(gamePhase);
            writer.Write(gameState);
            writer.Write(setPlay);
            writer.Write(firstHalf);
            writer.Write(kickingTeam);
            writer.Write(secsRemaining);
            writer.Write(secondaryTime);

            foreach (TeamInfo aTeam in team)
            {
                writer.Write(aTeam.ToByteArray(false));
            }

            return stream;
        }

        public bool FromByteArray(MemoryStream stream)
        {
            BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);

            byte[] header = reader.ReadBytes(4);
            string headerString = Encoding.UTF8.GetString(header);
            isTrueData = headerString == GAMECONTROLLER_TRUEGAMEDATA_STRUCT_HEADER;

            if (reader.ReadByte() != GAMECONTROLLER_STRUCT_VERSION)
            {
                return false;
            }

            packetNumber = reader.ReadByte();
            playersPerTeam = reader.ReadByte();
            competitionPhase = reader.ReadByte();
            competitionType = reader.ReadByte();
            gamePhase = reader.ReadByte();
            gameState = reader.ReadByte();
            setPlay = reader.ReadByte();
            firstHalf = reader.ReadByte();
            kickingTeam = reader.ReadByte();
            secsRemaining = reader.ReadInt16();
            secondaryTime = reader.ReadInt16();

            foreach (TeamInfo t in team)
            {
                t.FromByteArray(reader);
            }

            return true;
        }
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            string temp;

            output.AppendLine("             Header: " + GAMECONTROLLER_STRUCT_HEADER);
            output.AppendLine("            Version: " + GAMECONTROLLER_STRUCT_VERSION);
            output.AppendLine("      Packet Number: " + (packetNumber & 0xFF));
            output.AppendLine("   Players per Team: " + playersPerTeam);

            switch (competitionPhase)
            {
                case COMPETITION_PHASE_ROUNDROBIN:
                    temp = "round robin";
                    break;
                case COMPETITION_PHASE_PLAYOFF:
                    temp = "playoff";
                    break;
                default:
                    temp = "undefined(" + competitionPhase + ")";
                    break;
            }
            output.AppendLine("   competitionPhase: " + temp);

            switch (competitionType)
            {
                case COMPETITION_TYPE_NORMAL:
                    temp = "normal";
                    break;
                case COMPETITION_TYPE_DYNAMIC_BALL_HANDLING:
                    temp = "dynamic ball handling";
                    break;
                default:
                    temp = "undefined(" + competitionType + ")";
                    break;
            }
            output.AppendLine("    competitionType: " + temp);

            switch (gamePhase)
            {
                case GAME_PHASE_NORMAL:
                    temp = "normal";
                    break;
                case GAME_PHASE_PENALTYSHOOT:
                    temp = "penaltyshoot";
                    break;
                case GAME_PHASE_OVERTIME:
                    temp = "overtime";
                    break;
                case GAME_PHASE_TIMEOUT:
                    temp = "timeout";
                    break;
                default:
                    temp = "undefined(" + gamePhase + ")";
                    break;
            }
            output.AppendLine("          gamePhase: " + temp);

            switch (gameState)
            {
                case STATE_INITIAL:
                    temp = "initial";
                    break;
                case STATE_READY:
                    temp = "ready";
                    break;
                case STATE_SET:
                    temp = "set";
                    break;
                case STATE_PLAYING:
                    temp = "playing";
                    break;
                case STATE_FINISHED:
                    temp = "finish";
                    break;
                default:
                    temp = "undefined(" + gameState + ")";
                    break;
            }
            output.AppendLine("          gameState: " + temp);

            switch (setPlay)
            {
                case SET_PLAY_NONE:
                    temp = "none";
                    break;
                case SET_PLAY_GOAL_KICK:
                    temp = "goal kick";
                    break;
                case SET_PLAY_PUSHING_FREE_KICK:
                    temp = "pushing free kick";
                    break;
                case SET_PLAY_CORNER_KICK:
                    temp = "corner kick";
                    break;
                case SET_PLAY_KICK_IN:
                    temp = "kick in";
                    break;
                case SET_PLAY_PENALTY_KICK:
                    temp = "penalty kick";
                    break;
                default:
                    temp = "undefined(" + setPlay + ")";
                    break;
            }
            output.AppendLine("            setPlay: " + temp);

            switch (firstHalf)
            {
                case C_TRUE:
                    temp = "true";
                    break;
                case C_FALSE:
                    temp = "false";
                    break;
                default:
                    temp = "undefined(" + firstHalf + ")";
                    break;
            }
            output.AppendLine("          firstHalf: " + temp);

            output.AppendLine("        kickingTeam: " + kickingTeam);
            output.AppendLine("      secsRemaining: " + secsRemaining);
            output.AppendLine("      secondaryTime: " + secondaryTime);

            return output.ToString();
        }
    }
}
