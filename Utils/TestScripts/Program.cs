using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GCInfo
{
    private string _title;

    private GameControlData gameControlData = new GameControlData();
    private readonly object consoleLock = new object();

    // Use a wildcard address for the target
    IPAddress targetIpAddress = IPAddress.Any;

    // Ports for different types of messages
    int monitorPort = 3636;
    int controlPort = 3838;
    int returnDataPort = 3939;
    int forwardedStatusPort = 3940;
    // Start is called before the first frame update

    // Create UDP clients for each port
    UdpClient monitorRequestClient;
    UdpClient controlClient;
    UdpClient forwardedStatusClient;

    ConsoleColor originalColor = Console.ForegroundColor;


    static void Main() {
        
        GCInfo gcInfo = new GCInfo();

        gcInfo.Start();
        while (true){
            gcInfo.Update();
        }
    }

    void Start()
    {
        Console.Clear(); // Clear the console
        Console.WriteLine("Start");
        controlClient = new UdpClient(controlPort);
        forwardedStatusClient = new UdpClient(forwardedStatusPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, controlPort);
        byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
        Console.WriteLine(remoteEndPoint.Address.ToString());
        monitorRequestClient = new UdpClient();
        byte[] packet = Encoding.ASCII.GetBytes("RGTr\0");
        monitorRequestClient.Send(packet, packet.Length, new IPEndPoint(remoteEndPoint.Address, monitorPort));
        _title = "Monitor request sent.";
        monitorRequestClient.Close();
    }



    // Update is called once per frame
    // void Update()
    // {
    //     Console.WriteLine("Update");
    //     if (controlClient != null)
    //     {
    //         string data = ReceiveMessages(controlClient, targetIpAddress, controlPort, "RGTr", "Regular Control");
    //         if (data != "FAIL")
    //         {
    //             _title.text = data;
    //         }
    //     }
    // }

    void Update()
    {
        

        // Handle RGTr asynchronously
        Task.Run(() => HandleRGTrPacket("RGTr"));

        // Handle RGrt asynchronously
        Task.Run(() => HandleRGrtPacket("RGrt"));
    }

    async Task HandleRGTrPacket(string headerMagic)
    {
        if (controlClient != null)
        {
            GameControlData data = await Task.Run(() => ReceiveMessages(controlClient, targetIpAddress, controlPort, headerMagic, "Regular Control"));
            if (data != null)
            {
                // Handle the data accordingly
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Received {headerMagic}:\n");
                Console.ForegroundColor = originalColor;

                string output = gameControlData.ToString();
                StatusPrint(output, ConsoleColor.Blue);
                // Console.WriteLine($"Team 0:\n{data.team[0].ToString()}");
            }
        }
    }

     async Task HandleRGrtPacket(string headerMagic)
    {  
        Console.WriteLine("Start HandleRGrtPacket");
        if (forwardedStatusClient != null)
        {
            Console.WriteLine("====> await Task.Run");
            GameControlReturnData data = await Task.Run(() => ReceiveMessageGCReturnDataReceive(forwardedStatusClient, targetIpAddress, forwardedStatusPort, headerMagic, "GC Return Data"));
            Console.WriteLine("====> Got some data");
            if (data != null)
            {
                // Handle the data accordingly
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received {headerMagic}");
                Console.ForegroundColor = originalColor;
                // Console.WriteLine($"====> Received {headerMagic}");
            }
            // else {
            //     // Console.WriteLine("====> HandleRGrtPacket FAIL");
            // }
        }
    }

    void OnDestroy()
    {
        if (controlClient != null)
        {
            controlClient.Close();
        }

        if (monitorRequestClient != null)
        {
            monitorRequestClient.Close();
        }

        if (forwardedStatusClient != null)
        {
            forwardedStatusClient.Close();
        }
    }



    static GameControlData ReceiveMessages(UdpClient controlClient, IPAddress targetIpAddress, int targetPort, string headerMagic, string messageType)
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, targetPort);
        try
        {
            byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
            string receivedHeaderMagic = Encoding.ASCII.GetString(receivedData, 0, 4);
            string receivedMessage = Encoding.ASCII.GetString(receivedData);
            int byteArrayLength = receivedMessage.Length;
            string decodedMessage = Encoding.ASCII.GetString(receivedData, 5, byteArrayLength - 5);
            // Console.WriteLine($"{receivedHeaderMagic} Received: {receivedMessage} Extracted: {decodedMessage}");

            GameControlData data = new GameControlData();

            using (MemoryStream memoryStream = new MemoryStream(receivedData))
            {
                if (data.FromByteArray(memoryStream))
                {
                    if (data.isTrueData)
                    {

                        // Upate the Team Logos
                        // GameObject firstTeamLogoObject = GameObject.Find("FirstTeamLogo");
                        // if (firstTeamLogoObject != null)
                        // {
                            // LogoController firstTeamLogoController = firstTeamLogoObject.GetComponent<LogoController>();

                            // if (firstTeamLogoController != null)
                            // {
                                // firstTeamLogoController.SetTeamNumber(data.team[0].teamNumber);
                                Console.WriteLine($"Team Number: {data.team[0].teamNumber}");
                            // }
                            // else
                            // {
                            //     Console.WriteLine("LogoController component not found on the GameObject.");
                            // }
                        // }
                        // else
                        // {
                        //     Console.WriteLine("GameObject with the specified name not found.");
                        // }

                        // Upate the Team Logos
                        // GameObject secondTeamLogoObject = GameObject.Find("SecondTeamLogo");
                        // if (secondTeamLogoObject != null)
                        // {
                        //     LogoController secondTeamLogoController = secondTeamLogoObject.GetComponent<LogoController>();

                        //     if (secondTeamLogoController != null)
                        //     {
                                // secondTeamLogoController.SetTeamNumber(data.team[1].teamNumber);
                                Console.WriteLine($"Team Number: {data.team[1].teamNumber}");
                        //     }
                        //     else
                        //     {
                        //         Console.WriteLine("LogoController component not found on the GameObject.");
                        //     }
                        // }
                        // else
                        // {
                        //     Console.WriteLine("GameObject with the specified name not found.");
                        // }

                        StringBuilder output = new StringBuilder();
                        // output.AppendLine(data.ToString());
                        output.AppendLine("#############################################");
                        // string teamName0 = GetTeamName(data.team[0].teamNumber);
                        string teamName0 = "NAME0";
                        output.AppendLine($"Team Number: {data.team[0].teamNumber} Name: {teamName0}");
                        output.AppendLine($"Goals: {data.team[0].score}");
                        List<int> playerNums0 = new List<int> { 1, 2, 3, 4, 5 };
                        output.AppendLine(data.team[0].PlayersToSring(playerNums0));
                        output.AppendLine("#############################################");
                        // string teamName1 = GetTeamName(data.team[1].teamNumber);
                        string teamName1 = "NAME1";
                        output.AppendLine($"Team Number: {data.team[1].teamNumber} Name: {teamName1}");
                        output.AppendLine($"Goals: {data.team[1].score}");
                        List<int> playerNums1 = new List<int> { 1, 2, 3, 4, 5 };
                        output.AppendLine(data.team[1].PlayersToSring(playerNums1));
                        output.AppendLine("#############################################");
                        // Console.WriteLine(output.ToString());
                        // return (output.ToString());
                        return (data);
                    }
                }
                return null;
            }
        }
        catch (Exception err)
        {
            Console.WriteLine(err.ToString());
            // return "Exception Occured";
            return null;
        }
    }

    static GameControlReturnData ReceiveMessageGCReturnDataReceive(UdpClient controlClient, IPAddress targetIpAddress, int targetPort, string headerMagic, string messageType)
    {
        // Console.WriteLine("ReceiveMessageGCReturnDataReceive");
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, targetPort);
        try
        {
            // Console.WriteLine("====> Get some data");
            byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
            Console.WriteLine("====> Got some data");
            string receivedHeaderMagic = Encoding.ASCII.GetString(receivedData, 0, 4);
            string receivedMessage = Encoding.ASCII.GetString(receivedData);
            int byteArrayLength = receivedMessage.Length;
            string decodedMessage = Encoding.ASCII.GetString(receivedData, 5, byteArrayLength - 5);
            // Console.WriteLine($"{receivedHeaderMagic} WOW got a RGrt: {receivedMessage} Extracted: {decodedMessage}");
            Console.WriteLine("====> Got some Data");
            GameControlReturnData data = new GameControlReturnData();
            Console.WriteLine("====> Made a new GameControlReturnData");
            using (MemoryStream memoryStream = new MemoryStream(receivedData))
            {
                if (data.FromByteArray(memoryStream))
                {
                    return data;
                }
                Console.WriteLine("====> Failed to parse memory stream");
                return null;
            }
        }
        catch (Exception err)
        {
            Console.WriteLine(err.ToString());
            return null;
        }
    }







    static string GetTeamName(byte teamNumber)
    {
        Console.WriteLine("getTeamName");

        string[] teamNames = Teams.GetNames(true);

        if (teamNumber != null)
        {
            if (teamNumber < teamNames.Length && teamNames[teamNumber] != null)
            {
                return "Team " + teamNames[teamNumber];
            }
            else
            {
                return "Unknown" + teamNumber + ")";
            }
        }
        else
        {
            return "Unknown";
        }
    }

    void StatusPrint(string output, ConsoleColor color)
    {
        lock (consoleLock)
        {
            Console.SetCursorPosition(0, 0); // Set the cursor position to the top-left corner
            Console.ForegroundColor = color;
            Console.WriteLine(output);
            Console.ForegroundColor = originalColor;
        }
    }

}

