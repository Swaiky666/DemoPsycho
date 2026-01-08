using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    [Header("View Reference Settings")]
    [Tooltip("Place a child object here to define the camera's target position and rotation")]
    public Transform cameraViewPoint; 

    public Vector3 GetViewPosition()
    {
        // If no reference point, default to a top-down view offset
        return cameraViewPoint != null ? cameraViewPoint.position : transform.position + new Vector3(0, 5, -5);
    }

    public Quaternion GetViewRotation()
    {
        return cameraViewPoint != null ? cameraViewPoint.rotation : Quaternion.LookRotation(transform.position - GetViewPosition());
    }
}