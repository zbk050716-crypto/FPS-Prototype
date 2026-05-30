using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    public float mouseSensitivity = 2f;

    public Transform playerCamera;

    [Header("摄像头高度调整")]
    public float cameraHeight = 0.6f;

    private CharacterController controller;

    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;

        AdjustCameraHeight();
    }

    void AdjustCameraHeight()
    {
        if (playerCamera != null)
        {
            Vector3 localPos = playerCamera.localPosition;
            localPos.y = cameraHeight;
            playerCamera.localPosition = localPos;
        }
    }

    void Update()
    {
        Move();

        Look();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");

        float z = Input.GetAxis("Vertical");

        Vector3 move =
            transform.right * x +
            transform.forward * z;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;

        xRotation =
            Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
