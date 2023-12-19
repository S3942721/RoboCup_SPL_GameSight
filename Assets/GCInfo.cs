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
public class GCInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title;
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

    void Start()
    {
        Debug.Log("Start");
        controlClient = new UdpClient(controlPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, controlPort);
        byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
        Debug.Log(remoteEndPoint.Address.ToString());
        monitorRequestClient = new UdpClient();
        byte[] packet = Encoding.ASCII.GetBytes("RGTr\0");
        monitorRequestClient.Send(packet, packet.Length, new IPEndPoint(remoteEndPoint.Address, monitorPort));
        _title.text = "Monitor request sent.";
        monitorRequestClient.Close();
    }



    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update");
        if (controlClient != null)
        {
            string data = ReceiveMessages(controlClient, targetIpAddress, controlPort, "RGTr", "Regular Control");
            if (data != "FAIL")
            {
                _title.text = data;
            }
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



    static string ReceiveMessages(UdpClient controlClient, IPAddress targetIpAddress, int targetPort, string headerMagic, string messageType)
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, targetPort);
        try
        {
            byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
            string receivedHeaderMagic = Encoding.ASCII.GetString(receivedData, 0, 4);
            string receivedMessage = Encoding.ASCII.GetString(receivedData);
            int byteArrayLength = receivedMessage.Length;
            string decodedMessage = Encoding.ASCII.GetString(receivedData, 5, byteArrayLength - 5);
            Debug.Log($"{receivedHeaderMagic} Received: {receivedMessage} Extracted: {decodedMessage}");

            GameControlData data = new GameControlData();

            using (MemoryStream memoryStream = new MemoryStream(receivedData))
            {
                if (data.FromByteArray(memoryStream))
                {
                    if (data.isTrueData)
                    {

                        // Upate the Team Logos
                        GameObject firstTeamLogoObject = GameObject.Find("FirstTeamLogo");
                        if (firstTeamLogoObject != null){
                            LogoController firstTeamLogoController = firstTeamLogoObject.GetComponent<LogoController>();

                            if (firstTeamLogoController != null) {
                                firstTeamLogoController.SetTeamNumber(data.team[0].teamNumber);
                            }
                            else {
                                Debug.LogError("LogoController component not found on the GameObject.");
                            }
                        }
                        else {
                            Debug.LogError("GameObject with the specified name not found.");
                        }

                        // Upate the Team Logos
                        GameObject secondTeamLogoObject = GameObject.Find("SecondTeamLogo");
                        if (secondTeamLogoObject != null){
                            LogoController secondTeamLogoController = secondTeamLogoObject.GetComponent<LogoController>();

                            if (secondTeamLogoController != null) {
                                secondTeamLogoController.SetTeamNumber(data.team[1].teamNumber);
                            }
                            else {
                                Debug.LogError("LogoController component not found on the GameObject.");
                            }
                        }
                        else {
                            Debug.LogError("GameObject with the specified name not found.");
                        }

                        StringBuilder output = new StringBuilder();
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
                        output.AppendLine("#############################################");
                        Debug.Log(output.ToString());
                        return (output.ToString());
                    }
                }
                return "FAIL";
            }
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
            return "Exception Occured";
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

