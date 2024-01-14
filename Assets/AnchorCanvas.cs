using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AnchorCanvas : MonoBehaviour
{
    private ARRaycastManager arRaycastManager;
    private ARAnchorManager arAnchorManager;
    private ARPlaneManager arPlaneManager;

    private bool isDragging = false;
    private Vector2 touchStartPos;
    private Vector3 offset;

    private ARAnchor currentAnchor;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        arPlaneManager = GetComponent<ARPlaneManager>(); // Add this line to get the ARPlaneManager
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (TryGetTouchPosition(touch.position, out var touchPosition))
                    {
                        touchStartPos = touchPosition;
                        isDragging = true;

                        // Raycast to find a plane to place the anchor
                        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                        {
                            Pose hitPose = hits[0].pose;

                            // Create or update anchor at the hit position
                            if (currentAnchor == null)
                            {
                                currentAnchor = arAnchorManager.AddAnchor(hitPose);
                            }
                            else
                            {
                                currentAnchor.transform.position = hitPose.position;
                                currentAnchor.transform.rotation = hitPose.rotation;
                            }

                            transform.parent = currentAnchor.transform; // Parent canvas to anchor
                            offset = transform.position - hitPose.position;
                        }
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging && TryGetTouchPosition(touch.position, out var currentTouchPosition))
                    {
                        Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentTouchPosition.x, currentTouchPosition.y, 10f)) + offset;
                        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
                    }
                    break;

                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
    }

    bool TryGetTouchPosition(Vector2 touchPosition, out Vector2 result)
    {
        if (Camera.main != null)
        {
            result = Camera.main.ScreenToViewportPoint(touchPosition);
            return true;
        }
        else
        {
            result = Vector2.zero;
            return false;
        }
    }
}
