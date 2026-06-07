using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 50f;

    void Update()
    {
        // 使物体绕自身的 Y 轴以 rotationSpeed 的速度旋转
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
