using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public class TeamInfo
    {
        public const byte MAX_NUM_PLAYERS = 20;
        public const int SIZE = 1 + 1 + 1 + 1 + 1 + 1 + 2 + 2 + MAX_NUM_PLAYERS * PlayerInfo.SIZE;

        public byte teamNumber;
        public byte fieldPlayerColor;
        public byte goalkeeperColor;
        public byte goalkeeper = 1;
        public byte score;
        public byte penaltyShot = 0;
        public short singleShots = 0;
        public short messageBudget = 0;
        public readonly PlayerInfo[] player = new PlayerInfo[MAX_NUM_PLAYERS];

        public TeamInfo()
        {
            for (int i = 0; i < player.Length; i++)
            {
                player[i] = new PlayerInfo();
            }
        }

        public byte[] ToByteArray(bool decreaseScore)
        {
            MemoryStream stream = new MemoryStream(SIZE);
            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);

            writer.Write(teamNumber);
            writer.Write(fieldPlayerColor);
            writer.Write(goalkeeperColor);
            writer.Write(goalkeeper);
            writer.Write(decreaseScore ? (byte)(score - 1) : score);
            writer.Write(penaltyShot);
            writer.Write(singleShots);
            writer.Write(messageBudget);

            foreach (PlayerInfo info in player)
            {
                writer.Write(info.ToByteArray());
            }

            return stream.ToArray();
        }

        public void FromByteArray(BinaryReader reader)
        {
            teamNumber = reader.ReadByte();
            fieldPlayerColor = reader.ReadByte();
            goalkeeperColor = reader.ReadByte();
            goalkeeper = reader.ReadByte();
            score = reader.ReadByte();
            penaltyShot = reader.ReadByte();
            singleShots = reader.ReadInt16();
            messageBudget = reader.ReadInt16();

            foreach (PlayerInfo info in player)
            {
                info.FromByteArray(reader);
            }
        }

        public static string GetTeamColorName(byte teamColor)
        {
            switch (teamColor)
            {
                case GameControlData.TEAM_BLUE:
                    return "blue";
                case GameControlData.TEAM_RED:
                    return "red";
                case GameControlData.TEAM_YELLOW:
                    return "yellow";
                case GameControlData.TEAM_BLACK:
                    return "black";
                case GameControlData.TEAM_WHITE:
                    return "white";
                case GameControlData.TEAM_GREEN:
                    return "green";
                case GameControlData.TEAM_ORANGE:
                    return "orange";
                case GameControlData.TEAM_PURPLE:
                    return "purple";
                case GameControlData.TEAM_BROWN:
                    return "brown";
                case GameControlData.TEAM_GRAY:
                    return "gray";
                default:
                    return "undefined(" + teamColor + ")";
            }
        }

        public string PlayersToSring(List<int> playerNums)
        {
            StringBuilder output = new StringBuilder();
            foreach (int playerNum in playerNums)
            {
                output.AppendFormat("P: {0} - {1} | ", playerNum, player[playerNum-1].penalty);
            }
            output.AppendLine("");

            return output.ToString();

        }
        public override string ToString()
        {
            StringBuilder output = new StringBuilder("--------------------------------------\n");

            output.Append("         teamNumber: ").Append(teamNumber).Append("\n");
            output.Append("   fieldPlayerColor: ").Append(GetTeamColorName(fieldPlayerColor)).Append("\n");
            output.Append("    goalkeeperColor: ").Append(GetTeamColorName(goalkeeperColor)).Append("\n");
            output.Append("         goalkeeper: ").Append(goalkeeper).Append("\n");
            output.Append("              score: ").Append(score).Append("\n");
            output.Append("        penaltyShot: ").Append(penaltyShot).Append("\n");
            output.Append("        singleShots: ").Append(Convert.ToString(singleShots, 2)).Append("\n");
            output.Append("      messageBudget: ").Append(messageBudget).Append("\n");

            for (int i = 0; i < player.Length; i++)
            {
                output.Append("Player #").Append(i + 1).Append("\n").Append(player[i].ToString());
            }

            return output.ToString();
        }
    }
}
