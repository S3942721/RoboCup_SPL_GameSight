using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LogoController : MonoBehaviour
{
    public int teamNumber;
    public Image logoImage;

    public void SetTeamNumber(int newTeamNumber) {
        teamNumber = newTeamNumber;
        UpdateLogo();
    }

    void Start() {
        // Set team number to 0 before we start
        teamNumber = 0;
        UpdateLogo();
    }

    void UpdateLogo()
    {
        // Assuming logos are in a folder named "Logos" under "Assets/Resources"
        string logoPath = "Logos/" + teamNumber.ToString();
        // Debug.Log("Loading Team Logo: " + logoPath);

        // Load the logo dynamically
        Sprite logoSprite =  Resources.Load<Sprite>(logoPath);


        // Check if the sprite was loaded successfully
        if (logoSprite != null)
        {
            // Set the sprite for the Image component
            logoImage.sprite = logoSprite;
        }
        else
        {
            Debug.LogError("Logo not found for team number: " + teamNumber);
        }
    }
}
