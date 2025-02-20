using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PhysicsManager physics;
    [SerializeField] private Crouch crouch;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float coyoteJumpTime;
    [SerializeField] private float movementBonus;
    [SerializeField] private float crouchMultiplier;

    [Header("Runtime")]
    [SerializeField] private bool jumpAction;
    [SerializeField] private float jumpCooldownTimer;
    private bool readyToJump => jumpCooldownTimer <= 0 && Time.time - physics.LastGroundTime <= coyoteJumpTime;

    [Header("Gravity")]
    public float maxFallSpeed;

    public void GetInput(InputAction.CallbackContext context)
    {
        if(readyToJump) JumpAction();
    }

    private void Update()
    {
        if(!CheckComponents()) return;

        if(jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        ResetExitingSlope();
    }

    private void FixedUpdate()
    {
        if(!CheckComponents()) return;

        SetGravityUse();
        LimitFallSpeed();
    }

    private void JumpAction()
    {
        float speed = rb.linearVelocity.magnitude;
        float bonusForce = speed * movementBonus;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * (jumpForce + bonusForce) * (crouch.IsCrouching ? crouchMultiplier : 1f), ForceMode.VelocityChange);

        jumpCooldownTimer = jumpCooldown;

        physics.ExitingSlope = true;
    }

    private void ResetExitingSlope()
    {
        if(!physics.OnGround || jumpAction || jumpCooldownTimer > 0) return;

        physics.ExitingSlope = false;
    }

    private bool CheckComponents()
    {
        bool integrity = true;

        if(physics == null)
        {
            Debug.LogError("Physics Manager isn't assigned");
            integrity = false;
        }

        if(rb == null)
        {
            Debug.LogError("Rigidbody isn't assigned");
            integrity = false;
        }

        if(crouch == null)
        {
            Debug.LogError("Crouch Component isn't assigned");
            integrity = false;
        }

        return integrity;
    }

    private void SetGravityUse()
    {
        rb.useGravity = !physics.OnSlope;
    }

    private void LimitFallSpeed()
    {
        if(physics.OnSlope) return;

        if(Mathf.Abs(rb.linearVelocity.y) > maxFallSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Sign(rb.linearVelocity.y) * maxFallSpeed, rb.linearVelocity.z);
    }
}
