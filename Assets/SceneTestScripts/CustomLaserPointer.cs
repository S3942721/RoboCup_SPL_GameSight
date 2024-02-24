using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Collections;

public enum LeftDisplayModes
{
    Default,
    Off,
    Scoreboard
}

public enum RightDisplayModes
{
    Default,
    Off,
    Team0,
    Team1
}

public enum DisplayModes
{
    Left,
    Right
}

public class CustomLaserPointer : MonoBehaviour
{
    public OVRInput.Controller m_controller;
    [SerializeField]
    private Transform controllerAnchor;

    [SerializeField]
    private TextMeshProUGUI displayText = null;
    [SerializeField]
    private TextMeshProUGUI debugText = null;

    [SerializeField]
    private List<GameObject> displayObjects = new List<GameObject>();

    [Header("Controller Button Actions")]

    [SerializeField]
    private OVRInput.RawButton spawnObject;
    [SerializeField]
    private OVRInput.RawButton cycleText;
    [SerializeField]
    private OVRInput.RawButton toggleGCTrueDataButton;

    [SerializeField]
    GameObject spawnablePrefab;
    [SerializeField]
    GameObject spawnablePrefabGhost;
    [SerializeField]
    GameObject PassiveGCInfo;
    [SerializeField]
    GameObject ActiveGCInfo;

    private GameObject previewObject;

    GameObject spawnedObject;

    [Header("Line Render Settings")]
    [SerializeField]
    private float lineWidth = 0.01f;

    [SerializeField]
    private float lineMaxLength = 50f;

    private RaycastHit firstHit;
    private RaycastHit secondHit;
    private bool firstHitSet = false;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        spawnedObject = null;
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = lineWidth;

        // Instantiate the preview object
        if (spawnablePrefabGhost != null)
        {
            previewObject = Instantiate(spawnablePrefabGhost, Vector3.zero, Quaternion.identity);
            previewObject.SetActive(false); // Set it inactive initially
        }
    }

    void Update()
    {
        // Maybe can add this back in later to stop line renderer when controller not in hand.
        // Currently not working 
        // OVRInput.Hand handOfController = (m_controller == OVRInput.Controller.LTouch)
        //     ? OVRInput.Hand.HandLeft
        //     : OVRInput.Hand.HandRight;
        // OVRInput.ControllerInHandState controllerInHandState = OVRInput.GetControllerIsInHandState(handOfController);
        // if (controllerInHandState != OVRInput.ControllerInHandState.ControllerInHand)
        //     {
        //         lineRenderer.enabled = false;
        //     }
        //     else {
        //         lineRenderer.enabled = true;
        //     }

        Vector3 anchorPosition = controllerAnchor.position;
        Quaternion anchorRotation = controllerAnchor.rotation;

        if (Physics.Raycast(new Ray(anchorPosition, anchorRotation * Vector3.forward), out RaycastHit hit, lineMaxLength))
        {
            GameObject objectHit = hit.transform.gameObject;

            OVRSemanticClassification classification = objectHit?.GetComponentInParent<OVRSemanticClassification>();

            if (classification != null && classification.Labels?.Count > 0)
            {
                displayText.text = classification.Labels[0];
            }
            else
            {
                displayText.text = string.Empty;
            }

            lineRenderer.SetPosition(0, anchorPosition);
            lineRenderer.SetPosition(1, hit.point);

            if (OVRInput.GetDown(spawnObject))
            {

                if (firstHitSet)
                {
                    // get second hit
                    secondHit = hit;
                    // debugText.text = "secondHit set: " + hit.ToString();
                    SpawnSomething(firstHit, secondHit);
                    firstHitSet = false;
                }
                else
                {
                    firstHit = hit;
                    // debugText.text = "firstHit set: " + hit.ToString();
                    firstHitSet = true;
                }

            }

            // Update the preview object's position during the preview phase
            if (firstHitSet && spawnablePrefabGhost != null)
            {
                UpdatePreviewPosition(firstHit, hit);
            }

            if (OVRInput.GetDown(cycleText))
            {
                CycleDisplay();

            }

            if (OVRInput.GetDown(toggleGCTrueDataButton))
            {
                StartCoroutine(toggleGCTrueData());
            }

        }
        else
        {
            displayText.text = string.Empty;

            lineRenderer.SetPosition(0, anchorPosition);
            lineRenderer.SetPosition(1, anchorPosition + anchorRotation * Vector3.forward * lineMaxLength);
        }

    }



    void SpawnSomething(RaycastHit firstHit, RaycastHit secondHit)
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }

        // Calculate position (average of two points)
        Vector3 position = (firstHit.point + secondHit.point) / 2f;

        if (spawnablePrefab.CompareTag("Wall"))
        {
            // Calculate rotation based on the hit normal
            Quaternion rotation = Quaternion.LookRotation(firstHit.normal, Vector3.up) * Quaternion.Euler(0, 180, 0);

            // Calculate the scale based on the distance between two points
            float scale = Vector3.Distance(firstHit.point, secondHit.point);

            // Instantiate prefab with adjusted position, rotation, and scale
            spawnedObject = Instantiate(spawnablePrefab, position, rotation);
            spawnedObject.transform.localScale = new Vector3(spawnedObject.transform.localScale.x, spawnedObject.transform.localScale.y, scale);
        }
        else if (spawnablePrefab.CompareTag("Floor"))
        {
            // Calculate position for the bottom-left corner
            Vector3 spawnPos = new Vector3(firstHit.point.x, firstHit.point.y, firstHit.point.z);

            // Calculate the scale based on the distance between two points
            float scale = Vector3.Distance(new Vector3(firstHit.point.x, 0, firstHit.point.z), new Vector3(secondHit.point.x, 0, secondHit.point.z));

            // float scaleX = Mathf.Abs(secondHit.point.x - firstHit.point.x);
            // float scaleY = 1f;
            // float scaleZ = Mathf.Abs(secondHit.point.z - firstHit.point.z);

            // Instantiate prefab with adjusted position and scale
            spawnedObject = Instantiate(spawnablePrefab, spawnPos, Quaternion.identity);

            // Calculate the direction vector between firstHit and secondHit
            Vector3 direction = secondHit.point - firstHit.point;

            // Calculate the rotation to make the positive x,z corner at secondHit
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            // Apply rotation to the instantiated object
            spawnedObject.transform.rotation = rotation;

            // Apply scale to the instantiated object
            spawnedObject.transform.localScale = new Vector3(scale, 1f, scale);

        }

        if (previewObject != null)
        {
            previewObject.SetActive(false);
        }
    }

    private void UpdatePreviewPosition(RaycastHit startHit, RaycastHit currentHit)
    {
        // Calculate position (average of two points)
        Vector3 spawnPos = new Vector3(startHit.point.x, startHit.point.y, startHit.point.z);

        // Update the preview object's position
        previewObject.transform.position = spawnPos;

        // Calculate the scale based on the distance between two points
        float scale = Vector3.Distance(new Vector3(startHit.point.x, 0, startHit.point.z), new Vector3(currentHit.point.x, 0, currentHit.point.z));

        // float scaleX = Mathf.Abs(currentHit.point.x - startHit.point.x);
        // float scaleY = 1f;
        // float scaleZ = Mathf.Abs(currentHit.point.z - startHit.point.z);

        // Calculate the direction vector between firstHit and secondHit
        Vector3 direction = currentHit.point - startHit.point;

        // Calculate the rotation to make the positive x,z corner at secondHit
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        // Update the preview object's rotation
        previewObject.transform.rotation = rotation;

        // Update the preview object's scale
        previewObject.transform.localScale = new Vector3(scale, 1f, scale);

        // Set the preview object active
        previewObject.SetActive(true);


    }

    private void CycleDisplay()
    {
        int currentIndex = displayObjects.IndexOf(displayObjects.Find(obj => obj.activeSelf));

        // Deactivate the current object
        displayObjects[currentIndex].SetActive(false);

        // Activate the next object in the list (looping back to the start if at the end)
        int nextIndex = (currentIndex + 1) % displayObjects.Count;
        displayObjects[nextIndex].SetActive(true);
    }

    private IEnumerator toggleGCTrueData()
    {
        if (ActiveGCInfo.activeSelf)
        {
            ActiveGCInfo.SetActive(false);
            yield return new WaitForSeconds(1);
            PassiveGCInfo.SetActive(true);
        }
        else
        {
            PassiveGCInfo.SetActive(false);
            yield return new WaitForSeconds(1);
            ActiveGCInfo.SetActive(true);
        }
    }


}
