using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTextToCamera : MonoBehaviour
{
    public Transform targetCamera; // Reference to the OVRCameraRig or any other camera

    private void Update()
    {
        // Calculate the direction from the Text to the camera
        Vector3 directionToCamera = targetCamera.position - transform.position;

        // Calculate the rotation needed to align the Text with the camera's position
        Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera, Vector3.up);

        // Apply the rotation only along the Y-axis (flipped for readability)
        transform.rotation = Quaternion.Euler(0f, rotationToCamera.eulerAngles.y + 180f, 0f);
    }
}

