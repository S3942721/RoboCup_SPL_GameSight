using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;


namespace Data
{
    [Serializable]
    public class GameControlReturnData
    {
        public const int GAMECONTROLLER_RETURNDATA_PORT = 3939;
        public const int GAMECONTROLLER_RETURNDATA_FORWARD_PORT = GAMECONTROLLER_RETURNDATA_PORT + 1;
        public const string GAMECONTROLLER_RETURN_STRUCT_HEADER = "RGrt";
        public const byte GAMECONTROLLER_RETURN_STRUCT_VERSION = 4;
        public const int SIZE = 4 + 1 + 1 + 1 + 12 + 4 + 8;

        public string header { get; set; }
        public byte version { get; set; }
        public byte playerNum { get; set; }
        public byte teamNum { get; set; }
        public bool fallen { get; set; }
        public float[] pose { get; } = new float[3];
        public float ballAge { get; set; }
        public float[] ball { get; } = new float[2];

        public bool valid { get; private set; }
        public bool headerValid { get; private set; }
        public bool versionValid { get; private set; }
        public bool playerNumValid { get; private set; }
        public bool teamNumValid { get; private set; }
        public bool fallenValid { get; private set; }
        public bool poseValid { get; private set; }
        public bool ballValid { get; private set; }

        public byte[] ToByteArray()
        {
            using (MemoryStream stream = new MemoryStream(SIZE))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(Encoding.UTF8.GetBytes(header));
                writer.Write(version);
                writer.Write(playerNum);
                writer.Write(teamNum);
                writer.Write(fallen ? (byte)1 : (byte)0);
                writer.Write(pose[0]);
                writer.Write(pose[1]);
                writer.Write(pose[2]);
                writer.Write(ballAge);
                writer.Write(ball[0]);
                writer.Write(ball[1]);
                return stream.ToArray();
            }
        }

        public bool FromByteArray(MemoryStream stream)
        {
            BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);
            byte[] ipAddressBytes = reader.ReadBytes(4);
            string ipAddress = new IPAddress(ipAddressBytes).ToString();
            // Console.WriteLine($"Robot ip: {ipAddress}");
            byte[] header = reader.ReadBytes(4);
            string headerString = Encoding.UTF8.GetString(header);
            // Console.WriteLine($"**headerString: {headerString}");
            if (headerString == GAMECONTROLLER_RETURN_STRUCT_HEADER)
            {
                headerValid = true;

                version = reader.ReadByte();
                if (version == GAMECONTROLLER_RETURN_STRUCT_VERSION)
                {
                    versionValid = true;

                    playerNum = reader.ReadByte();
                    playerNumValid = playerNum >= 1 && playerNum <= TeamInfo.MAX_NUM_PLAYERS;

                    teamNum = reader.ReadByte();
                    teamNumValid = teamNum > 0;

                    fallen = reader.ReadByte() != 0;
                    fallenValid = fallen || !fallen;

                    pose[0] = reader.ReadSingle();
                    pose[1] = reader.ReadSingle();
                    pose[2] = reader.ReadSingle();
                    poseValid = !float.IsNaN(pose[0]) && !float.IsNaN(pose[1]) && !float.IsNaN(pose[2]);

                    ballAge = reader.ReadSingle();

                    ball[0] = reader.ReadSingle();
                    ball[1] = reader.ReadSingle();
                    ballValid = !float.IsNaN(ballAge) && !float.IsNaN(ball[0]) && !float.IsNaN(ball[1]);
                }
            }

            valid = headerValid && versionValid && playerNumValid && teamNumValid && fallenValid && poseValid && ballValid;

            return valid;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Header: {header}");
            sb.AppendLine($"Version: {version}");
            sb.AppendLine($"Player Number: {playerNum}");
            sb.AppendLine($"Team Number: {teamNum}");
            sb.AppendLine($"Fallen: {fallen}");
            sb.AppendLine($"Pose: [{pose[0]}, {pose[1]}, {pose[2]}]");
            sb.AppendLine($"Ball Age: {ballAge}");
            sb.AppendLine($"Ball: [{ball[0]}, {ball[1]}]");
            sb.AppendLine($"Valid: {valid}");
            sb.AppendLine($"Header Valid: {headerValid}");
            sb.AppendLine($"Version Valid: {versionValid}");
            sb.AppendLine($"Player Number Valid: {playerNumValid}");
            sb.AppendLine($"Team Number Valid: {teamNumValid}");
            sb.AppendLine($"Fallen Valid: {fallenValid}");
            sb.AppendLine($"Pose Valid: {poseValid}");
            sb.AppendLine($"Ball Valid: {ballValid}");

            return sb.ToString();
        }
    }
}

