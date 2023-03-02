using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MouseView : MonoBehaviour
{
    [Header("Define speed of rotation x and y of the player")]
    public float horizontalMouseSpeed = 400f;
    public float verticalMouseSpeed = 300f;
    [Header("Player reference")]
    [Tooltip("Reference to player transform")]
    public Transform playerObject;
    [Space]
    [Tooltip("Reference to wallRide in player scripts")]
    public WallRide wallRideComponent;
    public float _xRotation { get; private set; }

    private Camera characterCamera;

    void Start()
    {
        characterCamera = GetComponent<Camera>();
        _xRotation = 0f;
    }

    void Update()
    {
        if (DialogueManager.SharedInstance.isDialogue)
            return;
        float mouseX = Input.GetAxis("Mouse X") * horizontalMouseSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * verticalMouseSpeed * Time.deltaTime;
        playerObject.Rotate(Vector3.up * mouseX);

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        if (wallRideComponent != null)
        {
            transform.localEulerAngles = new Vector3(_xRotation, 0, wallRideComponent.GetCameraRoll());
        }
        else
        {
            transform.localEulerAngles = new Vector3(_xRotation, 0, 0);
        }
    }
}
