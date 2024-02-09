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

[Serializable]
public class GCInfo : MonoBehaviour
{
    public TMP_Text _title;
    public TMP_Text team0ScoreText;
    public TMP_Text team1ScoreText;
    public TMP_Text timerText;

    public GameObject player0_1;
    public GameObject player0_2;
    public GameObject player0_3;
    public GameObject player0_4;
    public GameObject player0_5;
    public GameObject player1_1;
    public GameObject player1_2;
    public GameObject player1_3;
    public GameObject player1_4;
    public GameObject player1_5;

    public GameObject[] team0Players;
    public GameObject[] team1Players;



    [SerializeField]
    public GameControlData gameControlData = new GameControlData();
    [SerializeField]
    public GameControlReturnData gameControlReturnData = new GameControlReturnData();
    private bool updateReady = false;
    private bool returnUpdateReady = false;
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

        // Dynamically find and assign TMP_Text objects
        try {
            _title = GameObject.Find("TitleText").GetComponent<TMP_Text>();
            team0ScoreText = GameObject.Find("Team0ScoreText").GetComponent<TMP_Text>();
            team1ScoreText = GameObject.Find("Team1ScoreText").GetComponent<TMP_Text>();
            timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();
        } catch {
            Debug.Log("Scoredboard not present");
        }

        try {

            // Dynamically find and assign player GameObjects
            player0_1 = GameObject.Find("Player0-1");
            player0_2 = GameObject.Find("Player0-2");
            player0_3 = GameObject.Find("Player0-3");
            player0_4 = GameObject.Find("Player0-4");
            player0_5 = GameObject.Find("Player0-5");
            player1_1 = GameObject.Find("Player1-1");
            player1_2 = GameObject.Find("Player1-2");
            player1_3 = GameObject.Find("Player1-3");
            player1_4 = GameObject.Find("Player1-4");
            player1_5 = GameObject.Find("Player1-5");

            team0Players = new GameObject[] { player0_1, player0_2, player0_3, player0_4, player0_5 };
            team1Players = new GameObject[] { player1_1, player1_2, player1_3, player1_4, player1_5 };
        } catch {
            Debug.Log("Players not present");
        }



        controlClient = new UdpClient(controlPort);
        forwardedStatusClient = new UdpClient(forwardedStatusPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, controlPort);
        byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
        Debug.Log(remoteEndPoint.Address.ToString());
        monitorRequestClient = new UdpClient();
        byte[] packet = Encoding.ASCII.GetBytes("RGTr\0");
        monitorRequestClient.Send(packet, packet.Length, new IPEndPoint(remoteEndPoint.Address, monitorPort));
        if (_title != null) {
            _title.text = "Monitor request sent.";
        }
        else {
            Debug.Log("Monitor request sent.");
        }
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
        // Debug.Log("Update()");
        if (updateReady){
            if (_title == null){
                try {
                    // Dynamically find and assign TMP_Text objects
                    _title = GameObject.Find("TitleText").GetComponent<TMP_Text>();
                    team0ScoreText = GameObject.Find("Team0ScoreText").GetComponent<TMP_Text>();
                    team1ScoreText = GameObject.Find("Team1ScoreText").GetComponent<TMP_Text>();
                    timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();
                } catch {
                    Debug.Log("Scoreboard not present");
                }
            }

            

            
            if (_title != null){
                _title.text = gameControlData.ToString();
            }

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
            
            if (team0ScoreText != null){
                UpdateScores();
                UpdateTimer();
            }
            updateReady = false;
        }

        // Debug.Log("Checking if returnUpdateReady ");
        // Debug.Log(returnUpdateReady);
        if (returnUpdateReady){
            // Debug.Log("returnUpdateReady"); 
            if (player0_1 == null){
                try {
                    // Dynamically find and assign player GameObjects
                    player0_1 = GameObject.Find("Player0-1");
                    player0_2 = GameObject.Find("Player0-2");
                    player0_3 = GameObject.Find("Player0-3");
                    player0_4 = GameObject.Find("Player0-4");
                    player0_5 = GameObject.Find("Player0-5");
                    player1_1 = GameObject.Find("Player1-1");
                    player1_2 = GameObject.Find("Player1-2");
                    player1_3 = GameObject.Find("Player1-3");
                    player1_4 = GameObject.Find("Player1-4");
                    player1_5 = GameObject.Find("Player1-5");

                    team0Players = new GameObject[] { player0_1, player0_2, player0_3, player0_4, player0_5 };
                    team1Players = new GameObject[] { player1_1, player1_2, player1_3, player1_4, player1_5 };
                } catch {
                    Debug.Log("Players not present");
                }
            }

            if (player0_1 != null && gameControlReturnData != null)
            {
                Debug.Log("Moving players based on gameControlReturnData ");
                int playerIndex = gameControlReturnData.playerNum - 1;
                if (gameControlReturnData.teamNumValid) {
                    Debug.Log("PLayer index:" + playerIndex);
                    GameObject currentPlayer;
                    if (gameControlReturnData.teamNum == 0){
                        currentPlayer = team0Players[playerIndex];
                    }
                    else {
                        currentPlayer = team1Players[playerIndex];
                    }
                    
                    Debug.Log(currentPlayer);

                    if (currentPlayer != null)
                    {
                        // Get the Transform component of the player
                        Transform playerTransform = currentPlayer.transform;

                        // Check if pose data is valid before updating the position
                        if (gameControlReturnData.poseValid)
                        {
                            GameObject fieldObject = GameObject.Find("Field"); 
                            if (fieldObject != null) {
                                Transform fieldTransform = fieldObject.transform;
                                Vector3 fieldRelativePosition = new Vector3(gameControlReturnData.pose[0], 0.25f, gameControlReturnData.pose[2]);
                                playerTransform.position = fieldTransform.TransformPoint(fieldRelativePosition);
                            }
                            // Assign the pose from gameControlReturnData to the player's position
                            // playerTransform.position = new Vector3(gameControlReturnData.pose[0]/1000, 0.25f, gameControlReturnData.pose[1]/1000);
                        }
                    }
                }
                else {
                    Debug.Log("Not valid playernumber");
                    // Debug.Log("Moving Team0 players based randomly ");
                    // foreach (GameObject player in team0Players)
                    // {
                    //     // Get the Transform component of the player
                    //     Transform playerTransform = player.transform;

                    //     GameObject fieldObject = GameObject.Find("Field"); 
                    //     if (fieldObject != null) {
                    //         Transform fieldTransform = fieldObject.transform;
                    //         // Modify the position (you can adjust these values as needed)
                    //         float newX = UnityEngine.Random.Range(-2.3f, 2.3f); // Example: random X position between -5 and 5
                    //         float newY = .25f;
                    //         float newZ = UnityEngine.Random.Range(-1.53f, 1.53f); // Example: random Z position between -5 and 5
                    //         Vector3 fieldRelativePosition = new Vector3(newX, newY, newZ);
                    //         playerTransform.position = fieldTransform.TransformPoint(fieldRelativePosition);
                    //     }
                    // }
                }
            returnUpdateReady = false;
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
                    // Debug.Log($"Received {headerMagic}: {gameControlData.ToString()}");
                    updateReady = true;

                    
                }
            }
        }
    }

    async Task HandleRGrtPacket(string headerMagic)
    {
        while(receivePackets){
        // Debug.Log("Start HandleRGrtPacket");
            if (forwardedStatusClient != null)
            {
                // Debug.Log("====> await Task.Run");
                GameControlReturnData data = await Task.Run(() => ReceiveMessageGCReturnDataReceive(forwardedStatusClient, targetIpAddress, forwardedStatusPort, headerMagic, "GC Return Data"));
                // Debug.Log("====> Got some data");
                if (data != null)
                {
                    gameControlReturnData = data;
                    // Debug.Log($"Received {headerMagic}:\n{data.ToString()}");
                    returnUpdateReady = true;
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

    void UpdateScores()
    {
        // Check if gameControlData is not null before accessing its members
        if (gameControlData != null)
        {
            // Assuming team[0] and team[1] represent the two teams
            if (gameControlData.team != null && gameControlData.team.Length >= 2)
            {
                // Debug.Log("Team0 Score!:" + gameControlData.team[0].score.ToString());
                team0ScoreText.text = gameControlData.team[0].score.ToString();
                team1ScoreText.text = gameControlData.team[1].score.ToString();
            }
            else
            {
                Debug.LogError("gameControlData.team is null or has insufficient length.");
            }
        }
        else
        {
            Debug.LogError("gameControlData is null.");
        }
    }

    void UpdateTimer()
    {
        // Check if gameControlData is not null before accessing its members
        if (gameControlData != null)
        {
            // Assuming secsRemaining represents the time in seconds
            // Debug.Log("Timer!:" + gameControlData.secsRemaining);
            int minutes = gameControlData.secsRemaining / 60;
            int seconds = Math.Abs(gameControlData.secsRemaining % 60);

            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
        else
        {
            Debug.LogError("gameControlData is null.");
        }
    }
}

