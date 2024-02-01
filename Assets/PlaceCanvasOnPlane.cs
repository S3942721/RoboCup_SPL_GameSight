using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceCanvasOnPlane : MonoBehaviour
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
        arPlaneManager = GetComponent<ARPlaneManager>();
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

                        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                        {
                            ARPlane plane = arPlaneManager.GetPlane(hits[0].trackableId);

                            if (plane != null && IsHorizontal(plane))
                            {
                                Pose hitPose = hits[0].pose;

                                if (currentAnchor == null)
                                {
                                    currentAnchor = arAnchorManager.AddAnchor(hitPose);
                                }
                                else
                                {
                                    currentAnchor.transform.position = hitPose.position;
                                    currentAnchor.transform.rotation = hitPose.rotation;
                                }

                                float canvasHeight = GetComponent<RectTransform>().rect.height;
                                Vector3 newPosition = new Vector3(hitPose.position.x, hitPose.position.y - canvasHeight / 2f, hitPose.position.z);

                                transform.position = newPosition;
                                offset = transform.position - hitPose.position;
                            }
                        }
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging && TryGetTouchPosition(touch.position, out var currentTouchPosition))
                    {
                        // Use the ARRaycastHit pose for positioning
                        if (arRaycastManager.Raycast(currentTouchPosition, hits, TrackableType.PlaneWithinPolygon))
                        {
                            Pose hitPose = hits[0].pose;
                            Vector3 newPosition = new Vector3(hitPose.position.x, hitPose.position.y - offset.y, hitPose.position.z);
                            transform.position = newPosition;
                        }
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

    bool IsHorizontal(ARPlane plane)
    {
        Vector3 planeNormal = plane.normal;
        Vector3 expectedUpVector = Vector3.up;

        return Vector3.Dot(planeNormal, expectedUpVector) > 0.9f;
    }
}
