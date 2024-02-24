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
    public TMP_Text scoreboardPlate;
    public TMP_Text gcToStringPlate;

    public TMP_Text rgrtInfoPlateP0_1;
    public TMP_Text rgrtInfoPlateP0_2;
    public TMP_Text rgrtInfoPlateP0_3;
    public TMP_Text rgrtInfoPlateP0_4;
    public TMP_Text rgrtInfoPlateP0_5;

    public TMP_Text rgrtInfoPlateP1_1;
    public TMP_Text rgrtInfoPlateP1_2;
    public TMP_Text rgrtInfoPlateP1_3;
    public TMP_Text rgrtInfoPlateP1_4;
    public TMP_Text rgrtInfoPlateP1_5;

    public TMP_Text player1_3Text;
    public TMP_Text player1_4Text;

    public TMP_Text rgrtInfoPlate;
    public TMP_Text rgrtInfoLastTime;

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

    public TMP_Text[] team0InfoPlates;
    public TMP_Text[] team1InfoPlates;


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

    int player0_1_move_index = 0;
    int player0_2_move_index = 0;
    int player1_1_move_index = 0;
    int player1_2_move_index = 0;

    public float moveSpeed = 0.01f;



    // Create arrays and interpolate positions
    Vector3[] vectorArray1;
    Vector3[] vectorArray2;
    Vector3[] vectorArray3;
    Vector3[] vectorArray4;

    void Start()
    {
        // Define the starting and ending points for each array
        Vector3 start1 = new Vector3(-2.7f, 0.25f, -1.74f);
        Vector3 end1 = new Vector3(-2.7f, 0.25f, 1.74f);

        Vector3 start2 = new Vector3(-2.7f, 0.25f, 1.74f);
        Vector3 end2 = new Vector3(2.7f, 0.25f, -1.74f);

        Vector3 start3 = new Vector3(2.7f, 0.25f, -1.74f);
        Vector3 end3 = new Vector3(2.7f, 0.25f, 1.74f);

        Vector3 start4 = new Vector3(2.7f, 0.25f, 1.74f);
        Vector3 end4 = new Vector3(-2.7f, 0.25f, -1.74f);

        vectorArray1 = CreateInterpolatedArray(start1, end1, 10);
        vectorArray2 = CreateInterpolatedArray(start2, end2, 10);
        vectorArray3 = CreateInterpolatedArray(start3, end3, 10);
        vectorArray4 = CreateInterpolatedArray(start4, end4, 10);




        Debug.Log("DEBUG_DISPLAY:Start GCInfo");

        // Dynamically find and assign TMP_Text objects
        try
        {
            _title = GameObject.Find("TitleText").GetComponent<TMP_Text>();
            team0ScoreText = GameObject.Find("Team0ScoreText").GetComponent<TMP_Text>();
            team1ScoreText = GameObject.Find("Team1ScoreText").GetComponent<TMP_Text>();
            timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();
            scoreboardPlate = GameObject.Find("ScoreboardPlate").GetComponent<TMP_Text>();
            gcToStringPlate = GameObject.Find("GCToStringPlate").GetComponent<TMP_Text>();

        }
        catch
        {
            Debug.Log("GCINFO:Scoredboard not present");
        }

        try
        {

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
        }
        catch
        {
            Debug.Log("GCINFO:Players not present");
        }


        try
        {
            rgrtInfoPlateP0_1 = GameObject.Find("rgrtInfoPlateP0_1").GetComponent<TMP_Text>();
            rgrtInfoPlateP0_2 = GameObject.Find("rgrtInfoPlateP0_2").GetComponent<TMP_Text>();
            rgrtInfoPlateP0_3 = GameObject.Find("rgrtInfoPlateP0_3").GetComponent<TMP_Text>();
            rgrtInfoPlateP0_4 = GameObject.Find("rgrtInfoPlateP0_4").GetComponent<TMP_Text>();
            rgrtInfoPlateP0_5 = GameObject.Find("rgrtInfoPlateP0_5").GetComponent<TMP_Text>();
            rgrtInfoPlateP1_1 = GameObject.Find("rgrtInfoPlateP1_1").GetComponent<TMP_Text>();
            rgrtInfoPlateP1_2 = GameObject.Find("rgrtInfoPlateP1_2").GetComponent<TMP_Text>();
            rgrtInfoPlateP1_3 = GameObject.Find("rgrtInfoPlateP1_3").GetComponent<TMP_Text>();
            rgrtInfoPlateP1_4 = GameObject.Find("rgrtInfoPlateP1_4").GetComponent<TMP_Text>();
            rgrtInfoPlateP1_5 = GameObject.Find("rgrtInfoPlateP1_5").GetComponent<TMP_Text>();

            team0InfoPlates = new TMP_Text[] { rgrtInfoPlateP0_1, rgrtInfoPlateP0_2, rgrtInfoPlateP0_3, rgrtInfoPlateP0_4, rgrtInfoPlateP0_5 };

            team1InfoPlates = new TMP_Text[] { rgrtInfoPlateP1_1, rgrtInfoPlateP1_2, rgrtInfoPlateP1_3, rgrtInfoPlateP1_4, rgrtInfoPlateP1_5 };
        }
        catch
        {
            Debug.Log("GCINFO:rgrtInfoPlates not present");
        }


        controlClient = new UdpClient(controlPort);
        forwardedStatusClient = new UdpClient(forwardedStatusPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(targetIpAddress, controlPort);
        byte[] receivedData = controlClient.Receive(ref remoteEndPoint);
        Debug.Log("GCINFO:" + remoteEndPoint.Address.ToString());
        monitorRequestClient = new UdpClient();
        byte[] packet = Encoding.ASCII.GetBytes("RGTr\0");
        monitorRequestClient.Send(packet, packet.Length, new IPEndPoint(remoteEndPoint.Address, monitorPort));
        if (_title != null)
        {
            _title.text = "Monitor request sent.";
        }
        else
        {
            Debug.Log("GCINFO:Monitor request sent.");
        }
        monitorRequestClient.Close();

        try
        {
            firstTeamLogoController = GameObject.Find("FirstTeamLogo").GetComponent<LogoController>();
        }
        catch
        {
            Debug.LogError("GCINFO:LogoController with the specified name not found.");
        }

        try
        {
            secondTeamLogoController = GameObject.Find("SecondTeamLogo").GetComponent<LogoController>();
        }
        catch
        {
            Debug.LogError("GCINFO:LogoController with the specified name not found.");
        }

        // Handle RGTr asynchronously
        Task.Run(() => HandleRGTrPacket("RGTr"));

        // Handle RGrt asynchronously
        Task.Run(() => HandleRGrtPacket("RGrt"));
    }


    void Update()
    {
        // Debug.Log("Update()");
        if (updateReady)
        {
            if (_title == null)
            {
                try
                {
                    // Dynamically find and assign TMP_Text objects
                    _title = GameObject.Find("TitleText").GetComponent<TMP_Text>();
                    team0ScoreText = GameObject.Find("Team0ScoreText").GetComponent<TMP_Text>();
                    team1ScoreText = GameObject.Find("Team1ScoreText").GetComponent<TMP_Text>();
                    timerText = GameObject.Find("TimerText").GetComponent<TMP_Text>();
                }
                catch
                {
                    Debug.Log("GCINFO:Scoreboard not present");
                }
            }


            if (scoreboardPlate == null)
            {
                try
                {
                    scoreboardPlate = GameObject.Find("ScoreboardPlate").GetComponent<TMP_Text>();
                }
                catch
                {
                    Debug.Log("GCINFO:ScoreboardPlate not present");
                }
            }

            if (gcToStringPlate == null)
            {
                try
                {
                    gcToStringPlate = GameObject.Find("GCToStringPlate").GetComponent<TMP_Text>();
                }
                catch
                {
                    Debug.Log("GCINFO:GCToStringPlate not present");
                }
            }

            if (firstTeamLogoController == null)
            {
                try
                {
                    firstTeamLogoController = GameObject.Find("FirstTeamLogo").GetComponent<LogoController>();
                    secondTeamLogoController = GameObject.Find("SecondTeamLogo").GetComponent<LogoController>();
                }
                catch
                {
                    Debug.Log("GCINFO:LogoControllers not present");
                }
            }


            if (_title != null)
            {
                _title.text = gameControlData.ToString();
            }

            if (scoreboardPlate != null)
            {
                scoreboardPlate.text = gameControlData.getScoreBoard();
            }

            if (gcToStringPlate != null)
            {
                gcToStringPlate.text = "<align=left><mspace=.8em>" + gameControlData.ToString();
            }

            // Update the Team Logos
            if (firstTeamLogoController != null)
            {
                firstTeamLogoController.SetTeamNumber(gameControlData.team[0].teamNumber);
                secondTeamLogoController.SetTeamNumber(gameControlData.team[1].teamNumber);
            }


            if (team0ScoreText != null)
            {
                UpdateScores();
                UpdateTimer();
            }


            // // THIS IS FOR DEBUG ONLY
            // // Dynamically find and assign player GameObjects
            // player0_1 = GameObject.Find("Player0-1");
            // // player0_1_move_index = MovePlayer(player0_1, player0_1_move_index);
            // Transform player0_1Transform = player0_1.transform;
            // player0_1Transform.localPosition = new Vector3(-2.7f, 0.25f, -1.74f);

            // // Dynamically find and assign player GameObjects
            // player0_2 = GameObject.Find("Player0-2");
            // // player0_2_move_index = MovePlayer(player0_2, player0_2_move_index);
            // Transform player0_2Transform = player0_2.transform;
            // player0_2Transform.localPosition = new Vector3(-2.7f, 0.25f, 1.74f);

            // // Dynamically find and assign player GameObjects
            // player1_1 = GameObject.Find("Player1-1");
            // // player1_1_move_index = MovePlayer(player1_1, player1_1_move_index);
            // Transform player1_1Transform = player1_1.transform;
            // player1_1Transform.localPosition = new Vector3(-1*2.7f, 0.25f, -1*-1.74f);

            // // Dynamically find and assign player GameObjects
            // player1_2 = GameObject.Find("Player1-2");
            // // player1_2_move_index = MovePlayer(player1_2, player1_2_move_index);
            // Transform player1_2Transform = player1_2.transform;
            // player1_2Transform.localPosition = new Vector3(-1*2.7f, 0.25f, -1*1.74f);






            updateReady = false;
        }

        if (returnUpdateReady)
        {
            Debug.Log("DEBUG_DISPLAY: returnUpdateReady");

            if (player0_1 == null)
            {
                try
                {
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
                }
                catch
                {
                    Debug.Log("GCINFO:Players not present");
                }
            }
            if (rgrtInfoPlateP0_1 == null)
            {
                try
                {
                    rgrtInfoPlateP0_1 = GameObject.Find("rgrtInfoPlateP0_1").GetComponent<TMP_Text>();
                    rgrtInfoPlateP0_2 = GameObject.Find("rgrtInfoPlateP0_2").GetComponent<TMP_Text>();
                    rgrtInfoPlateP0_3 = GameObject.Find("rgrtInfoPlateP0_3").GetComponent<TMP_Text>();
                    rgrtInfoPlateP0_4 = GameObject.Find("rgrtInfoPlateP0_4").GetComponent<TMP_Text>();
                    rgrtInfoPlateP0_5 = GameObject.Find("rgrtInfoPlateP0_5").GetComponent<TMP_Text>();
                    rgrtInfoPlateP1_1 = GameObject.Find("rgrtInfoPlateP1_1").GetComponent<TMP_Text>();
                    rgrtInfoPlateP1_2 = GameObject.Find("rgrtInfoPlateP1_2").GetComponent<TMP_Text>();
                    rgrtInfoPlateP1_3 = GameObject.Find("rgrtInfoPlateP1_3").GetComponent<TMP_Text>();
                    rgrtInfoPlateP1_4 = GameObject.Find("rgrtInfoPlateP1_4").GetComponent<TMP_Text>();
                    rgrtInfoPlateP1_5 = GameObject.Find("rgrtInfoPlateP1_5").GetComponent<TMP_Text>();

                    team0InfoPlates = new TMP_Text[] { rgrtInfoPlateP0_1, rgrtInfoPlateP0_2, rgrtInfoPlateP0_3, rgrtInfoPlateP0_4, rgrtInfoPlateP0_5 };

                    team1InfoPlates = new TMP_Text[] { rgrtInfoPlateP1_1, rgrtInfoPlateP1_2, rgrtInfoPlateP1_3, rgrtInfoPlateP1_4, rgrtInfoPlateP1_5 };
                }
                catch
                {
                    Debug.Log("GCINFO:rgrtInfoPlates not present");
                }
            }


            if (player0_1 != null && gameControlReturnData != null)
            {

                Debug.Log("GCINFO:Moving players based on gameControlReturnData ");
                Debug.Log("DEBUG_DISPLAY: returnUpdateReady for player: " + gameControlReturnData.playerNum.ToString());
                int playerIndex = gameControlReturnData.playerNum - 1;
                if (gameControlReturnData.teamNumValid)
                {

                    Debug.Log("GCINFO:PLayer index:" + playerIndex);
                    GameObject currentPlayer;
                    TMP_Text currentPlayerPlate;

                    int poseMultiple = 1;

                    if (gameControlReturnData.teamNum == gameControlData.team[0].teamNumber)
                    {
                        currentPlayer = team0Players[playerIndex];
                        try {
                            currentPlayerPlate = team0InfoPlates[playerIndex];
                        } catch {
                            Debug.Log("Error setting current PlayerPlate");
                            currentPlayerPlate = null;
                        }

                    }
                    else
                    {
                        currentPlayer = team1Players[playerIndex];
                        try {
                            currentPlayerPlate = team1InfoPlates[playerIndex];
                        } catch {
                            Debug.Log("Error setting current PlayerPlate");
                            currentPlayerPlate = null;
                        }
                        poseMultiple = -1;
                    }

                    Debug.Log("GCINFO:" + currentPlayer);

                    if (currentPlayerPlate != null)
                    {
                        currentPlayerPlate.text = gameControlReturnData.ToString();
                    }

                    if (currentPlayer != null)
                    {
                        // Get the Transform component of the player
                        Transform playerTransform = currentPlayer.transform;

                        // Check if pose data is valid before updating the position
                        if (gameControlReturnData.poseValid)
                        {
                            playerTransform.localPosition = new Vector3(poseMultiple * gameControlReturnData.pose[0] / 1000, 0, poseMultiple * gameControlReturnData.pose[1] / 1000);
                        }
                    }
                }
                else
                {
                    Debug.Log("GCINFO:Not valid playernumber");
                }
                returnUpdateReady = false;
            }
        }
    }


    async Task HandleRGTrPacket(string headerMagic)
    {
        while (receivePackets)
        {
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
        while (receivePackets)
        {
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
            Debug.LogError("GCINFO:Caught Exception during ReceiveMessages");
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
            Console.WriteLine("GCINFO:====> Got some Data");
            GameControlReturnData data = new GameControlReturnData();

            using (MemoryStream memoryStream = new MemoryStream(receivedData))
            {
                if (data.FromByteArray(memoryStream))
                {
                    return data;
                }
                Debug.LogError("GCINFO:====> Failed to parse memory stream");
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
        Console.WriteLine("GCINFO:getTeamName");

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
                Debug.LogError("GCINFO:gameControlData.team is null or has insufficient length.");
            }
        }
        else
        {
            Debug.LogError("GCINFO:gameControlData is null.");
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
            Debug.LogError("GCINFO:gameControlData is null.");
        }
    }

    // Function to create an array of interpolated Vector3 positions between start and end
    Vector3[] CreateInterpolatedArray(Vector3 start, Vector3 end, int count)
    {
        Vector3[] result = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1); // Interpolation parameter between 0 and 1
            result[i] = Vector3.Lerp(start, end, t);
        }

        return result;
    }

    // Function to move the player along vectorArray1
    int MovePlayer(GameObject player, int index)
    {
        int currentPositionIndex = index;
        if (currentPositionIndex < vectorArray1.Length)
        {
            // Move towards the current position in vectorArray1
            player.transform.position = Vector3.MoveTowards(player.transform.position, vectorArray1[currentPositionIndex], moveSpeed * Time.deltaTime);

            // Check if the player has reached the current position
            if (Vector3.Distance(player.transform.position, vectorArray1[currentPositionIndex]) < 0.01f)
            {
                currentPositionIndex++; // Move to the next position
            }
        }
        else
        {
            // All positions reached, reset the index or perform any other necessary actions
            currentPositionIndex = 0;
        }
        return currentPositionIndex;
    }

}

