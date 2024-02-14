using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugDisplay : MonoBehaviour
{
    // Use a List to store the last 5 log messages
    private List<string> lastLogMessages = new List<string>();
    
    public TextMeshProUGUI display;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Add the log message to the list
        lastLogMessages.Add(logString);

        // Limit the list size to 5
        while (lastLogMessages.Count > 5)
        {
            lastLogMessages.RemoveAt(0); // Remove the oldest log message
        }

        // Construct the display text from the last 5 log messages
        string displayText = "";
        for (int i = lastLogMessages.Count - 1; i >= 0; i--)
        {
            displayText += lastLogMessages[i] + "\n";
        }

        // Update the TextMeshProUGUI component
        display.text = displayText;
    }
}
