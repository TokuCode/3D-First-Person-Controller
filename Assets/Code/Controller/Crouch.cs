using UnityEngine;
using UnityEngine.InputSystem;

public class Crouch : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PhysicsManager physics;

    [Header("Runtime")]
    [SerializeField] private float startYScale;
    [SerializeField] private bool isCrouching;
    public bool IsCrouching => isCrouching;

    [Header("Input")]
    [SerializeField] private bool crouchInput;

    [Header("Parameters")]
    [SerializeField] private float crouchYScale;

    public void GetInput(InputAction.CallbackContext context)
    {
        if(context.performed) crouchInput = true;
        else if(context.canceled) crouchInput = false;
    }

    private void Awake()
    {
        if(!CheckComponents()) return;

        startYScale = player.localScale.y;
    }

    private void FixedUpdate()
    {
        if(crouchInput && !isCrouching)
            StartCrouching();
        else if(!crouchInput && isCrouching && !physics.BlockedHead)
            EndCrouching();
    }

    private void StartCrouching()
    {
        player.localScale = new Vector3(player.localScale.x, crouchYScale, player.localScale.z);
        rb.AddForce(-Vector3.up * 5f, ForceMode.VelocityChange);

        isCrouching = true;
    }

    private void EndCrouching()
    {
        player.localScale = new Vector3(player.localScale.x, startYScale, player.localScale.z);

        isCrouching = false;
    }

    private bool CheckComponents()
    {
        bool integrity = true;

        if(player == null)
        {
            Debug.LogError("Player Transform isn't assigned");
            integrity = false;
        }

        if(rb == null)
        {
            Debug.LogError("Rigidbody isn't assigned");
            integrity = false;
        }

        if(physics == null)
        {
            Debug.LogError("Physics Manager isn't assigned");
            integrity = false;
        }

        return integrity;
    }
}
