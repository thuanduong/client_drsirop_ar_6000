using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 50.0f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
