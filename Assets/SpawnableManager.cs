using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SpawnableManager : MonoBehaviour
{
    [SerializeField]
    ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    [SerializeField]
    GameObject spawnablePrefab;

    Camera arCam;
    GameObject spawnedObject;

    // Start is called before the first frame update
    void Start()
{
    spawnedObject = null;

    // Find the "AR Camera" GameObject
    GameObject arCameraObject = GameObject.Find("AR Camera");

    // Check if the GameObject is found
    if (arCameraObject != null)
    {
        // Get the Camera component
        arCam = arCameraObject.GetComponent<Camera>();

        // Check if the Camera component is found
        if (arCam == null)
        {
            Debug.LogError("AR Camera does not have a Camera component.");
        }
    }
    else
    {
        Debug.LogError("AR Camera GameObject not found.");
    }
}


    // Update is called once per frame
    void Update()
{
    if (Input.touchCount == 0)
    {
        return;
    }

    if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Destroy the existing spawned object before creating a new one
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }

            if (m_Hits[0].trackable.gameObject.tag == "Spawnable")
            {
                spawnedObject = m_Hits[0].trackable.gameObject;
            }
            else
            {
                SpawnPrefab(m_Hits[0].pose.position);
            }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // If there is an existing spawned object, move it
            if (spawnedObject != null)
            {
                spawnedObject.transform.position = m_Hits[0].pose.position;
            }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            // No need to set spawnedObject to null since it will be destroyed on Began
        }
    }
}

    private void SpawnPrefab(Vector3 spawnPosition)
    {
        spawnedObject = Instantiate(spawnablePrefab, spawnPosition, Quaternion.identity);
    }
}
