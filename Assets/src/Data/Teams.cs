using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Data
{
    public class Teams
    {
        private class Info
        {
            public readonly string Name;
            // public Bitmap Icon;
            public readonly string[] Colors;

            public Info(string name, string[] colors)
            {
                Name = name;
                Colors = colors;
            }
        }

        private const string PATH = "config/";
        private const string CONFIG = "teams.cfg";
        private const string CHARSET = "UTF-8";
        private static readonly string[] PIC_ENDING = { "png", "gif", "jpg", "jpeg" };

        private static readonly Teams instance = new Teams();

        private readonly Info[][] teams;

        private Teams()
        {
            teams = new Info[Rules.Leagues.Length][];
            for (int i = 0; i < Rules.Leagues.Length; i++)
            {
                string dir = Rules.Leagues[i].leagueDirectory;
                int maxValue = 0;
                StreamReader br = null;
                Console.WriteLine(PATH + dir + "/" +  CONFIG);
                try
                {   
                    using (Stream inStream = File.OpenRead(PATH + dir + "/" + CONFIG))
                    {
                        br = new StreamReader(inStream, Encoding.GetEncoding(CHARSET));
                        string line;
                        while ((line = br.ReadLine()) != null)
                        {
                            try
                            {
                                int value = int.Parse(line.Split('=')[0]);
                                if (value > maxValue)
                                {
                                    maxValue = value;
                                }
                            }
                            catch (FormatException)
                            {
                            }
                        }
                    }
                }
                catch (IOException)
                {
                    // Log.Error("cannot load " + PATH + dir + "/" + CONFIG);
                    Console.WriteLine("11cannot load " + PATH + dir + "/" + CONFIG);
                }
                finally
                {
                    br?.Close();
                }
                teams[i] = new Info[maxValue + 1];
            }
        }

        private static int GetLeagueIndex()
        {
            for (int i = 0; i < Rules.Leagues.Length; i++)
            {
                if (Rules.Leagues[i] == Rules.league)
                {
                    return i;
                }
            }
            // Log.Error("selected league is odd");
            Console.WriteLine("selected league is odd");
            return -1;
        }

        public static void ReadTeams()
        {
            StreamReader br = null;
            try
            {   
                Console.WriteLine(PATH + Rules.league.leagueDirectory + "/" + CONFIG);
                using (Stream inStream = File.OpenRead(PATH + Rules.league.leagueDirectory + "/" + CONFIG))
                {
                    br = new StreamReader(inStream, Encoding.GetEncoding(CHARSET));
                    string line;
                    while ((line = br.ReadLine()) != null)
                    {
                        string[] entry = line.Split('=');
                        if (entry.Length == 2)
                        {
                            if (int.TryParse(entry[0], out int key) && key >= 0)
                            {
                                string[] values = entry[1].Split(',');
                                instance.teams[GetLeagueIndex()][key] = new Info(
                                    values[0],
                                    values.Length >= 3 ? new string[] { values[1], values[2] } :
                                    values.Length == 2 ? new string[] { values[1] } : new string[0]);
                            }
                            else
                            {
                                // Log.Error("error in teams.cfg: \"" + entry[0] + "\" is not a valid team number");
                                Console.WriteLine("error in teams.cfg: \"" + entry[0] + "\" is not a valid team number");
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Log.Error("malformed entry in teams.cfg: \"" + line + "\"");
                            Console.WriteLine("malformed entry in teams.cfg: \"" + line + "\"");
                        }
                    }
                }
            }
            catch (IOException)
            {
                // Log.Error("cannot load " + PATH + Rules.league.leagueDirectory + "/" + CONFIG);
                Console.WriteLine("cannot load " + PATH + Rules.league.leagueDirectory + "/" + CONFIG);
            }
            finally
            {
                br?.Close();
            }
        }

        public static string[] GetNames(bool withNumbers)
        {
            Console.WriteLine("running GetNames");
            int leagueIndex = GetLeagueIndex();
            if (instance.teams[leagueIndex][0] == null)
            {
                ReadTeams();
            }
            string[] outArr = new string[instance.teams[leagueIndex].Length];
            for (int i = 0; i < instance.teams[leagueIndex].Length; i++)
            {
                if (instance.teams[leagueIndex][i] != null)
                {
                    outArr[i] = instance.teams[leagueIndex][i].Name + (withNumbers ? " (" + i + ")" : "");
                }
            }
            return outArr;
        }

        // private static void ReadIcon(int team)
        // {
        //     Bitmap outBitmap = null;
        //     FileInfo file = GetIconPath(team);
        //     if (file != null)
        //     {
        //         try
        //         {
        //             outBitmap = new Bitmap(file.FullName);
        //         }
        //         catch (IOException)
        //         {
        //             Log.Error("cannot load " + file.FullName);
        //         }
        //     }
        //     if (outBitmap == null)
        //     {
        //         outBitmap = new Bitmap(100, 100);
        //         using (Graphics graphics = Graphics.FromImage(outBitmap))
        //         {
        //             graphics.Clear(Color.Transparent);
        //         }
        //     }
        //     instance.teams[GetLeagueIndex()][team].Icon = outBitmap;
        // }

        public static FileInfo GetIconPath(int team)
        {
            foreach (string ending in PIC_ENDING)
            {
                FileInfo file = new FileInfo(PATH + Rules.league.leagueDirectory + "/" + team + "." + ending);
                if (file.Exists)
                {
                    return file;
                }
            }

            return null;
        }

        // public static Bitmap GetIcon(int team)
        // {
        //     int leagueIndex = GetLeagueIndex();
        //     if (instance.teams[leagueIndex][team] == null)
        //     {
        //         ReadTeams();
        //     }
        //     if (instance.teams[leagueIndex][team].Icon == null)
        //     {
        //         ReadIcon(team);
        //     }
        //     return instance.teams[leagueIndex][team].Icon;
        // }

        public static string[] GetColors(int team)
        {
            int leagueIndex = GetLeagueIndex();
            if (instance.teams[leagueIndex][team] == null)
            {
                ReadTeams();
            }
            return instance.teams[leagueIndex][team].Colors;
        }
    }
}
