using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraAnchor : MonoBehaviour
{
    [Header("View Reference")]
    public Transform cameraViewPoint; 

    [Header("Linked UI System")]
    // 拖入该物体上的 CoordinateUI 脚本
    public CoordinateUI linkedUI; 

    public Vector3 GetViewPosition() => cameraViewPoint != null ? cameraViewPoint.position : transform.position + new Vector3(0, 5, -5);
    public Quaternion GetViewRotation() => cameraViewPoint != null ? cameraViewPoint.rotation : Quaternion.LookRotation(transform.position - GetViewPosition());
}