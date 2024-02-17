using System;
using System.IO;
using System.Text;

namespace Data
{
    [Serializable]
    public class PlayerInfo
    {
        public const byte PENALTY_NONE = 0;
        public const byte PENALTY_SPL_ILLEGAL_BALL_CONTACT = 1;
        public const byte PENALTY_SPL_PLAYER_PUSHING = 2;
        public const byte PENALTY_SPL_ILLEGAL_MOTION_IN_SET = 3;
        public const byte PENALTY_SPL_INACTIVE_PLAYER = 4;
        public const byte PENALTY_SPL_ILLEGAL_POSITION = 5;
        public const byte PENALTY_SPL_LEAVING_THE_FIELD = 6;
        public const byte PENALTY_SPL_REQUEST_FOR_PICKUP = 7;
        public const byte PENALTY_SPL_LOCAL_GAME_STUCK = 8;
        public const byte PENALTY_SPL_ILLEGAL_POSITION_IN_SET = 9;
        public const byte PENALTY_SPL_PLAYER_STANCE = 10;
        public const byte PENALTY_SUBSTITUTE = 14;
        public const byte PENALTY_MANUAL = 15;

        public const int SIZE = 1 + 1;

        public byte penalty = PENALTY_NONE;
        public byte secsTillUnpenalised;

        public byte[] ToByteArray()
        {
            MemoryStream stream = new MemoryStream(SIZE);
            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);

            writer.Write(penalty);
            writer.Write(secsTillUnpenalised);

            return stream.ToArray();
        }

        public void FromByteArray(BinaryReader reader)
        {
            penalty = reader.ReadByte();
            secsTillUnpenalised = reader.ReadByte();
        }

        public static string GetPenaltyName(byte penalty)
        {
            switch (penalty)
            {
                case PENALTY_NONE:
                    return "none";
                case PENALTY_SPL_ILLEGAL_BALL_CONTACT:
                    return "illegal ball contact";
                case PENALTY_SPL_PLAYER_PUSHING:
                    return "pushing";
                case PENALTY_SPL_ILLEGAL_MOTION_IN_SET:
                    return "illegal motion in set";
                case PENALTY_SPL_INACTIVE_PLAYER:
                    return "inactive";
                case PENALTY_SPL_ILLEGAL_POSITION:
                    return "illegal position";
                case PENALTY_SPL_LEAVING_THE_FIELD:
                    return "leaving the field";
                case PENALTY_SPL_REQUEST_FOR_PICKUP:
                    return "request for pickup";
                case PENALTY_SPL_LOCAL_GAME_STUCK:
                    return "local game stuck";
                case PENALTY_SPL_ILLEGAL_POSITION_IN_SET:
                    return "illegal position in set";
                case PENALTY_SPL_PLAYER_STANCE:
                    return "player stance";
                case PENALTY_SUBSTITUTE:
                    return "substitute";
                case PENALTY_MANUAL:
                    return "manual";
                default:
                    return "undefined(" + penalty + ")";
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            string temp = GetPenaltyName(penalty);

            output.AppendLine("            penalty: " + temp);
            output.AppendLine("secsTillUnpenalised: " + secsTillUnpenalised);

            return output.ToString();
        }
    }
}
