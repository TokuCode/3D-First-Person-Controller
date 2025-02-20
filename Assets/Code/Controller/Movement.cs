using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PhysicsManager physics;
    [SerializeField] private Rigidbody rb;

    [Header("Movement Parameters")]
    [SerializeField] private float airMultiplier;
    [SerializeField] private float airFriction;
    [SerializeField] private float groundFriction;

    [Header("Movement Variables")]
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;

    [Header("Input")]
    [SerializeField] private Vector2 input;
    public bool isTurningForward => Vector3.Dot(rb.linearVelocity, orientation.forward) * input.y < 0f;
    public bool isTurningRight => Vector3.Dot(rb.linearVelocity, orientation.right) * input.x < 0f;

    public void GetInput(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if(!CheckComponents()) return;

        Move();
        TransitionSlopeToGround();
        Friction();
        LimitSpeed();
    }

    private void Move()
    {
        Vector3 direction = orientation.forward * input.y + orientation.right * input.x;

        if(physics.OnSlope && !physics.ExitingSlope)
        {
            direction = physics.ProjectOnSlopePlane(direction);
            rb.AddForce(direction.normalized * 20f * acceleration, ForceMode.Acceleration);

            float speedOnNormalDirection = Vector3.Dot(rb.linearVelocity, physics.SlopeHit.normal);
            if(speedOnNormalDirection > 0)
            {
                rb.AddForce(-physics.SlopeHit.normal * 40f, ForceMode.Acceleration);
            }

            return;
        }

        rb.AddForce(direction.normalized * 10f * (physics.OnGround ? 1f : airMultiplier) * acceleration, ForceMode.Acceleration);
    }

    private void LimitSpeed()
    {
        if(physics.OnSlope && !physics.ExitingSlope)
        {
            if(rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

            return;
        }

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if(flatVelocity.magnitude > maxSpeed)
        {
            flatVelocity = flatVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
        }
    }

    private void Friction()
    {
        Vector3 drag = Vector3.zero;
        float dragFactor = physics.OnGround ? groundFriction : airFriction;

        if(isTurningRight || input.x == 0) drag += Vector3.right * dragFactor;
        if(isTurningForward || input.y == 0) drag += Vector3.forward * dragFactor;

        ApplyDrag(drag);
    }

    private void ApplyDrag(Vector3 directionalDrag)
    {
        Vector3 velocityForward = Vector3.Project(rb.linearVelocity, orientation.forward);
        Vector3 velocityRight = Vector3.Project(rb.linearVelocity, orientation.right);
        Vector3 velocityUp = Vector3.Project(rb.linearVelocity, orientation.up);

        Vector3 dragForward = -velocityForward * directionalDrag.z * Time.fixedDeltaTime;
        Vector3 dragRight = -velocityRight * directionalDrag.x * Time.fixedDeltaTime;
        Vector3 dragUp = -velocityUp * directionalDrag.y * Time.fixedDeltaTime;

        rb.AddForce(dragForward + dragRight + dragUp, ForceMode.VelocityChange);
    }

    public void SetMaxSpeed(float maxSpeed) => this.maxSpeed = maxSpeed;
    public void SetAcceleration(float acceleration) => this.acceleration = acceleration;

    private bool CheckComponents()
    {
        bool integrity = true;

        if(orientation == null)
        {
            Debug.LogError("Player orientation isn't assigned");
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

    private void TransitionSlopeToGround()
    {
        if(physics.PreviousOnSlope && !physics.OnSlope && !physics.ExitingSlope)
        {
            rb.AddForce(-Vector3.up * 40f, ForceMode.VelocityChange);
        }
    }
}
