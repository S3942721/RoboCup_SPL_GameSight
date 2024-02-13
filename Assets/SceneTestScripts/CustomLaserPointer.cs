using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomLaserPointer : MonoBehaviour
{
    [SerializeField] private Transform controllerAnchor;
    [SerializeField] private TextMeshProUGUI displayText = null;

    [Header("Controller Button Actions")]
    [SerializeField] private OVRInput.RawButton meshRenderToggleAction;
    [SerializeField] private OVRInput.RawButton objectsToggleAction;
    [SerializeField] private OVRInput.RawButton restoreObjectsAction;

    private LineRenderer lineRenderer; // The line renderer for the laser pointer
    private RaycastHit hit; // The hit information from the raycast
    private GameObject spawnedObject; // The object that was spawned
    private Vector3? firstPoint = null; // The first point for the two-point spawning
    private Coroutine waitForSecondRaycastCoroutine = null; // The coroutine to wait for the second raycast
    private List<GameObject> processedObjects = new List<GameObject>(); // The list of processed objects

    private void Awake()
    {
        // Initialize the spawned object and line renderer
        spawnedObject = null;
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = lineWidth;
    }

    void Update()
    {
        // Get the position and rotation of the controller anchor
        Vector3 anchorPosition = controllerAnchor.position;
        Quaternion anchorRotation = controllerAnchor.rotation;

        // Cast a ray from the controller anchor in the direction it's pointing
        if (Physics.Raycast(new Ray(anchorPosition, anchorRotation * Vector3.forward), out hit, lineMaxLength))
        {
            // Get the object that was hit by the raycast
            GameObject objectHit = hit.transform.gameObject;

            // Update the display text and line renderer positions
            UpdateDisplayText(objectHit);
            UpdateLineRendererPositions(anchorPosition, hit.point);

            // Try to spawn an object if the spawn button is pressed
            if (OVRInput.GetDown(spawnObject))
            {
                TrySpawnObject(objectHit, hit);
            }
        }
    }

    // Update the display text with the classification labels of the hit object
    private void UpdateDisplayText(GameObject objectHit)
    {
        OVRSemanticClassification classification = objectHit?.GetComponentInParent<OVRSemanticClassification>();
        displayText.text = classification != null && classification.Labels?.Count > 0 ? classification.Labels[0] : string.Empty;
    }

    // Update the positions of the line renderer
    private void UpdateLineRendererPositions(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    // Try to spawn an object based on the hit object and hit information
    private void TrySpawnObject(GameObject objectHit, RaycastHit hit)
    {
        // Get the semantic classification of the hit object
        OVRSemanticClassification classification = objectHit?.GetComponentInParent<OVRSemanticClassification>();

        // If the spawnable prefab is a wall and the hit object is classified as a wall face, try to spawn something
        if (spawnablePrefab.CompareTag("Wall") && classification != null && classification.Contains(OVRSceneManager.Classification.WallFace))
        {
            SpawnSomething(hit);
        }
        // If the spawnable prefab is a floor and the hit object is classified as a floor, try to spawn something
        else if (spawnablePrefab.CompareTag("Floor") && classification != null && classification.Contains(OVRSceneManager.Classification.Floor))
        {
            SpawnSomething(hit);
        }
    }

    // Coroutine to wait for the second raycast
    IEnumerator WaitForSecondRaycast()
    {
        // Wait for 10 seconds
        yield return new WaitForSeconds(10);

        // If the second raycast didn't happen, spawn the prefab at the first point
        if (firstPoint != null)
        {
            SpawnPrefab(firstPoint.Value);
            firstPoint = null;
        }
    }

    // Position and scale the object between two points
    void PositionAndScaleObject(Vector3 point1, Vector3 point2)
    {
        // Destroy the existing spawned object before creating a new one
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }

        // Calculate the position, rotation, and scale
        Vector3 position = (point1 + point2) / 2;
        Quaternion rotation = Quaternion.LookRotation(point2 - point1);
        Vector3 scale = new Vector3(1, 1, (point2 - point1).magnitude);

        // Spawn the prefab and apply the position, rotation, and scale
        spawnedObject = SpawnPrefab(position);
        spawnedObject.transform.rotation = rotation;
        spawnedObject.transform.localScale = scale;
    }

    // Spawn a prefab at a given position
    GameObject SpawnPrefab(Vector3 position)
    {
        // Instantiate the prefab at the given position
        GameObject spawnedPrefab = Instantiate(prefabToSpawn, position, Quaternion.identity);
        return spawnedPrefab;
    }
}
