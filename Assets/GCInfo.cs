using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GCInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title;
    [SerializeField]
    public GameControlData gameControlData = new GameControlData();
    public GameControlReturnData gameControlReturnData = new GameControlReturnData();
    private bool updateReady = false;
    private bool receivePackets = true;

    // Use a wildcard address for the target
    IPAddress targetIpAddress = IPAddress.Any;
     // Keep track of scheduled tasks
    private List<Task> scheduledTasks = new List<Task>();

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
    
    LogoController firstTeamLogoController;
    LogoController secondTeamLogoController;

    void Start()
    {
        Debug.Log("Start");
        controlClient = new UdpClient(controlPort);
        forwardedStatusClient = new UdpClient(forwardedStatusPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, controlPort);
        byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
        Debug.Log(remoteEndPoint.Address.ToString());
        monitorRequestClient = new UdpClient();
        byte[] packet = Encoding.ASCII.GetBytes("RGTr\0");
        monitorRequestClient.Send(packet, packet.Length, new IPEndPoint(remoteEndPoint.Address, monitorPort));
        _title.text = "Monitor request sent.";
        monitorRequestClient.Close();
        GameObject firstTeamLogoObject = GameObject.Find("FirstTeamLogo");
        if (firstTeamLogoObject != null)
        {
            firstTeamLogoController = firstTeamLogoObject.GetComponent<LogoController>();
        }
        else
        {
            Debug.LogError("GameObject with the specified name not found.");
        }

        GameObject secondTeamLogoObject = GameObject.Find("SecondTeamLogo");
        if (secondTeamLogoObject != null)
        {
            secondTeamLogoController = secondTeamLogoObject.GetComponent<LogoController>();
        }
        else
        {
            Debug.LogError("GameObject with the specified name not found.");
        }

        // Handle RGTr asynchronously
        Task.Run(() => HandleRGTrPacket("RGTr"));

        // Handle RGrt asynchronously
        Task.Run(() => HandleRGrtPacket("RGrt"));
    }


    void Update()
    {
        if (updateReady){
            _title.text = gameControlData.ToString();
            updateReady = false;

            // Update the Team Logos
            if (firstTeamLogoController != null)
            {
                firstTeamLogoController.SetTeamNumber(gameControlData.team[0].teamNumber);
            }
            else
            {
                Debug.LogError("LogoController component not found on the GameObject.");
            }
            

            if (secondTeamLogoController != null)
            {
                secondTeamLogoController.SetTeamNumber(gameControlData.team[1].teamNumber);
            }
            else
            {
                Debug.LogError("LogoController component not found on the GameObject.");
            }
        }

        
    }


    async Task HandleRGTrPacket(string headerMagic)
    {
        while(receivePackets){
        if (controlClient != null)
            {   
                GameControlData data = await Task.Run(() => ReceiveMessages(controlClient, targetIpAddress, controlPort, headerMagic, "Regular Control"));
                if (data != null)
                {
                    // Handle the data accordingly
                    gameControlData = data;
                    Debug.Log($"Received {headerMagic}: {gameControlData.ToString()}");
                    updateReady = true;

                    
                }
            }
        }
    }

     async Task HandleRGrtPacket(string headerMagic)
    {
        while(receivePackets){
        Debug.Log("Start HandleRGrtPacket");
            if (forwardedStatusClient != null)
            {
                // Debug.Log("====> await Task.Run");
                GameControlReturnData data = await Task.Run(() => ReceiveMessageGCReturnDataReceive(forwardedStatusClient, targetIpAddress, forwardedStatusPort, headerMagic, "GC Return Data"));
                // Debug.Log("====> Got some data");
                if (data != null)
                {
                    gameControlReturnData = data;
                    Debug.Log($"Received {headerMagic}:\n{data.ToString()}");
                    updateReady = true;
                }
            }
        }
    }

    void OnDestroy()
    {   
        receivePackets = false;
        // Debug.Log("OnDestroy Called!");
        // // Wait for all scheduled tasks to complete
        // Task.WaitAll(scheduledTasks.ToArray());
        // Debug.Log("All Tasks completed");

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
            // Debug.Log($"{receivedHeaderMagic} Received: {receivedMessage} Extracted: {decodedMessage}");

            GameControlData data = new GameControlData();

            using (MemoryStream memoryStream = new MemoryStream(receivedData))
            {
                if (data.FromByteArray(memoryStream))
                {
                    if (data.isTrueData)
                    {
                        StringBuilder output = new StringBuilder();
                        output.AppendLine(data.ToString());
                        output.AppendLine("#############################################");
                        // string teamName0 = GetTeamName(data.team[0].teamNumber);
                        string teamName0 = "teamName0";
                        output.AppendLine($"Team Number: {data.team[0].teamNumber} Name: {teamName0}");
                        output.AppendLine($"Goals: {data.team[0].score}");
                        List<int> playerNums0 = new List<int> { 1, 2, 3, 4, 5 };
                        output.AppendLine(data.team[0].PlayersToSring(playerNums0));
                        output.AppendLine("#############################################");
                        // string teamName1 = GetTeamName(data.team[1].teamNumber);
                        string teamName1 = "teamName1";
                        output.AppendLine($"Team Number: {data.team[1].teamNumber} Name: {teamName1}");
                        output.AppendLine($"Goals: {data.team[1].score}");
                        List<int> playerNums1 = new List<int> { 1, 2, 3, 4, 5 };
                        output.AppendLine(data.team[1].PlayersToSring(playerNums1));
                        output.AppendLine("#############################################");
                        // Debug.Log(output.ToString());
                        return (data);
                    }
                }
                return null;
            }
        }
        catch (Exception err)
        {
            Debug.LogError("Caught Exception during ReceiveMessages");
            Debug.LogError(err.ToString());
            return null;
        }
    }

    static GameControlReturnData ReceiveMessageGCReturnDataReceive(UdpClient controlClient, IPAddress targetIpAddress, int targetPort, string headerMagic, string messageType)
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, targetPort);
        try
        {
            byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
            string receivedHeaderMagic = Encoding.ASCII.GetString(receivedData, 0, 4);
            string receivedMessage = Encoding.ASCII.GetString(receivedData);
            int byteArrayLength = receivedMessage.Length;
            string decodedMessage = Encoding.ASCII.GetString(receivedData, 5, byteArrayLength - 5);
            // Console.WriteLine($"{receivedHeaderMagic} WOW got a RGrt: {receivedMessage} Extracted: {decodedMessage}");
            Console.WriteLine("====> Got some Data");
            GameControlReturnData data = new GameControlReturnData();

            using (MemoryStream memoryStream = new MemoryStream(receivedData))
            {
                if (data.FromByteArray(memoryStream))
                {
                    return data;
                }
                Debug.LogError("====> Failed to parse memory stream");
                return null;
            }
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
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
}

