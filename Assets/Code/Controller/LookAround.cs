using UnityEngine;
using UnityEngine.InputSystem;

public class LookAround : MonoBehaviour
{
    [Header("Camera Components")]
    [SerializeField] private Transform cameraParent;
    [SerializeField] private Transform playerOrientation;

    [Header("Camera Rig")]
    [SerializeField] private Transform playerCameraPosition;
    [SerializeField] private float distanceToPlayer;

    [Header("Camera Coordinates")]
    [SerializeField] private float azimutal;
    [SerializeField] private float polar;
    [Space, SerializeField] private float minPolar;
    [SerializeField] private float maxPolar;

    [Header("Camera Movement")]
    [SerializeField] private Vector2 sensibility;
    [SerializeField, Range(0f, 1f)] private float smoothing;
    [SerializeField, Range(0f, 1f)] private float positionSmoothing;
    [SerializeField] private bool invertPolar;

    [Header("Input")]
    [SerializeField] private Vector2 mouseDelta;

    [Header("Cursor")]
    [SerializeField] private bool cursorVisible;
    [SerializeField] private CursorLockMode cursorLockMode;

    private void Start()
    {
        if(!CheckComponents()) return;

        GetCoordinatesFromPlayer();
        PlaceCamera(0f);
    }

    private void Update()
    {
        if(!CheckComponents()) return;

        MoveCamera();
        PlaceCamera(smoothing);

        SetCursor();
    }

    private void GetCoordinatesFromPlayer()
    {
        Quaternion playerRotation = playerCameraPosition.rotation;
        playerCameraPosition.rotation = Quaternion.identity;

        azimutal = playerRotation.eulerAngles.y;
        polar = playerRotation.eulerAngles.x;
    }

    private void PlaceCamera(float smoothing)
    {
        Quaternion actualRotation = cameraParent.rotation;
        Quaternion targetRotation = Quaternion.Euler(polar, azimutal, 0f);

        Quaternion finalRotation = Quaternion.Slerp(actualRotation, targetRotation, .5f + (1 - smoothing) / 2);

        Vector3 newCameraPosition = playerCameraPosition.position + finalRotation * Vector3.forward * distanceToPlayer;
        cameraParent.position = Vector3.Lerp(cameraParent.position, newCameraPosition, .5f + (1 - positionSmoothing) / 2);
        cameraParent.rotation = finalRotation;

        playerOrientation.rotation = Quaternion.Euler(0f, finalRotation.eulerAngles.y, 0f);
    }

    public void GetInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    private void MoveCamera()
    {
        if (mouseDelta == Vector2.zero) return;

        azimutal = (azimutal + mouseDelta.x * sensibility.x) % 360;
        polar = Mathf.Clamp(polar - (invertPolar ? -1 : 1) * mouseDelta.y * sensibility.y, minPolar, maxPolar);
    }

    private bool CheckComponents()
    {
        bool integrity = true;

        if(playerOrientation == null)
        {
            Debug.LogError("Player Orientation not assigned");
            integrity = false;
        }

        if(cameraParent == null)
        {
            Debug.LogError("Camera Parent not assigned");
            integrity = false;
        }

        if(playerCameraPosition == null)
        {
            Debug.LogError("Player Camera Position not assigned");
            integrity = false;
        }

        return integrity;
    }

    private void SetCursor()
    {
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorLockMode;
    }
}
