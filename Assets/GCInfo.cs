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
public class GCInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title;
    // Use a wildcard address for the target
    IPAddress targetIpAddress = IPAddress.Parse("192.168.40.157");

    // Ports for different types of messages
    int monitorPort = 3636;
    int controlPort = 3838;
    int returnDataPort = 3939;
    int forwardedStatusPort = 3940;
    // Start is called before the first frame update

    // Create UDP clients for each port
    UdpClient monitorRequestClient = new UdpClient();
    UdpClient controlClient = new UdpClient(3838);
    UdpClient forwardedStatusClient = new UdpClient();
    void Start()
    {
        Debug.Log("Start");
        // try
        // {
        // First send initial monitor reques
        byte[] packet = Encoding.ASCII.GetBytes("RGTr\0");
        monitorRequestClient.Send(packet, packet.Length, new IPEndPoint(targetIpAddress, monitorPort));
        _title.text = "Monitor request sent.";
        monitorRequestClient.Close();


        // // Listen for packets
        string data = ReceiveMessages(controlClient, targetIpAddress, 3636, "RGTr", "Regular Control");
        _title.text = data;
        Debug.Log("hmmmm " + data);
        // }
        // catch (Exception ex)
        // {
        //     Debug.LogError($"An error occurred: {ex.Message}");
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Update");/

        // // Listen for packets
        string data = ReceiveMessages(controlClient, targetIpAddress, 3636, "RGTr", "Regular Control");
        if (data != "FAIL")
        {
            _title.text = data;

        }
        // Debug.Log("hmmmm " + data);



    }

    void OnDestroy()
    {
        controlClient.Close();
        monitorRequestClient.Close();
        forwardedStatusClient.Close();
        // Add similar lines for other UdpClient instances if needed
    }



    static string ReceiveMessages(UdpClient controlClient, IPAddress targetIpAddress, int targetPort, string headerMagic, string messageType)
    {
        // Debug.Log("targetIpAddress:" + targetIpAddress + " targetPort:" + targetPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, targetPort);
        Debug.Log(remoteEndPoint.ToString());
        byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
        // Debug.Log("WOWOW");
        // Debug.Log(receivedData.ToString());
        string receivedHeaderMagic = Encoding.ASCII.GetString(receivedData, 0, 4);
        string receivedMessage = Encoding.ASCII.GetString(receivedData);
        int byteArrayLength = receivedMessage.Length;
        string decodedMessage = Encoding.ASCII.GetString(receivedData, 5, byteArrayLength - 5);
        // Console.WriteLine($"{receivedHeaderMagic} Received: {receivedMessage} Extracted: {decodedMessage}");

        GameControlData data = new GameControlData();

        using (MemoryStream memoryStream = new MemoryStream(receivedData))
        {
            // // Create a BinaryReader to read from the MemoryStream
            // using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            // {
            if (data.FromByteArray(memoryStream))
            {
                // Console.WriteLine("Looks like we got data!");

                if (data.isTrueData)
                {
                    StringBuilder output = new StringBuilder();
                    // timestampOfLastTrueGameControlData = System.currentTimeMillis();
                    // output.AppendLine("Data seems to be true data!");
                    output.AppendLine(data.ToString());
                    output.AppendLine("#############################################");
                    string teamName0 = GetTeamName(data.team[0].teamNumber);
                    output.AppendLine($"Team Number: {data.team[0].teamNumber} Name: {teamName0}");
                    output.AppendLine($"Goals: {data.team[0].score}");
                    List<int> playerNums0 = new List<int> { 1, 2, 3, 4, 5 };
                    output.AppendLine(data.team[0].PlayersToSring(playerNums0));
                    output.AppendLine("#############################################");
                    string teamName1 = GetTeamName(data.team[1].teamNumber);
                    output.AppendLine($"Team Number: {data.team[1].teamNumber} Name: {teamName1}");
                    output.AppendLine($"Goals: {data.team[1].score}");
                    List<int> playerNums1 = new List<int> { 1, 2, 3, 4, 5 };
                    output.AppendLine(data.team[1].PlayersToSring(playerNums1));

                    // Console.WriteLine(data.team[0].ToString());
                    // Console.WriteLine(data.team[1].ToString());
                    output.AppendLine("#############################################");
                    return (output.ToString());
                }
            }
            return "FAIL";
            // }
            // }
        }
    }


    static string GetTeamName(byte teamNumber)
    {
        Console.WriteLine("getTeamName");

        string[] teamNames = Teams.GetNames(true);

        // foreach (string s in teamNames) {
        //     Console.WriteLine(s);
        // }
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

