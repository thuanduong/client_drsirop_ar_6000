using UnityEngine;

public class BillboardComponent : MonoBehaviour
{
    public Canvas canvas;
    public Camera mainCamera;
    public Transform target;

    private Transform mainCameraTransform;

    public void SetCamera(Camera cam)
    {
        mainCamera = cam;
        mainCameraTransform = mainCamera.transform;
        canvas.worldCamera = cam;
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null && target != null)
        {
            target.transform.LookAt(target.transform.position + mainCameraTransform.rotation * Vector3.forward, mainCameraTransform.rotation * Vector3.up);
        }
    }
}
