using UnityEngine;
using UnityEngine.InputSystem;

public class Ladder : MonoBehaviour
{
    private const float EXTRA_OFFSET_RAYCAST_FORWARD = .1f;
    private const float EXTRA_OFFSET_RAYCAST_UP = .1f;
    private const float STEP_JUMP_EXTRA_DISTANCE = 0f;
    private const float MAX_ANGLE_OF_INTENTION = 30f;
    private const float STRAIGHT_WALL_MAX_ANGLE = 30f;

    [Header("Components")]
    [SerializeField] private Collider box;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform player;
    [SerializeField] private PhysicsManager physics;

    [Header("Runtime")]
    [SerializeField] private bool isPathBlocked;
    [SerializeField] private bool isPathLadderStep;
    [SerializeField] private RaycastHit ladderHit;
    [SerializeField] private RaycastHit stepHit;
    [SerializeField] private Vector3 moveDirection;

    [Header("Input")]
    [SerializeField] private Vector2 moveInput;

    [Header("Parameters")]
    [SerializeField] private float stepSize;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Debug")]
    [SerializeField] private bool debug;

    public void MoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if(!CheckComponents()) return;

        GetMoveDirection();

        CheckBlockedPath();
        CheckPathLadderJumpeable();

        JumpStep();
    }

    private void CheckBlockedPath()
    {
        Vector3 footPoint = player.position + Vector3.up * (EXTRA_OFFSET_RAYCAST_UP - box.bounds.extents.y);

        float footStepDistance = EXTRA_OFFSET_RAYCAST_FORWARD + box.bounds.extents.z;

        if(Physics.Raycast(footPoint, moveDirection, out stepHit, footStepDistance, whatIsGround))
        {
            float angle = Vector3.Angle(-moveDirection, stepHit.normal);

            isPathBlocked = angle <= STRAIGHT_WALL_MAX_ANGLE;
        }
        else isPathBlocked = false;
    }

    private void CheckPathLadderJumpeable()
    {
        float forwardDistanceRaycast = box.bounds.extents.z + EXTRA_OFFSET_RAYCAST_FORWARD;

        if(isPathBlocked)
        {
            forwardDistanceRaycast = (stepHit.point - (player.position - Vector3.up * (box.bounds.extents.y - EXTRA_OFFSET_RAYCAST_UP))).magnitude;
        }

        Vector3 footPoint = player.position + Vector3.up * (stepSize + EXTRA_OFFSET_RAYCAST_UP - box.bounds.extents.y)
            + moveDirection * (forwardDistanceRaycast + .05f);

        isPathLadderStep = Physics.Raycast(footPoint, Vector3.down, out ladderHit, stepSize + EXTRA_OFFSET_RAYCAST_UP - .05f, whatIsGround);
    }

    private void JumpStep()
    {
        if(physics.OnSlope) return;

        if(moveInput.magnitude <= 0f) return;

        Vector3 inputDirection = orientation.right * moveInput.x + orientation.forward * moveInput.y;
        float angleOfIntention = Vector3.Angle(moveDirection, inputDirection);

        if(angleOfIntention > MAX_ANGLE_OF_INTENTION) return;

        if(isPathBlocked && isPathLadderStep)
        {
            float stepHeight = ladderHit.point.y - player.position.y + STEP_JUMP_EXTRA_DISTANCE + box.bounds.extents.y;
            rb.position += Vector3.up * stepHeight;
        }
    }

    private void GetMoveDirection()
    {
        moveDirection = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).normalized;
    }

    private bool CheckComponents()
    {
        bool integrity = true;

        if(rb == null)
        {
            Debug.LogError("Rigidbody isn't assigned");
            integrity = false;
        }

        if(box == null)
        {
            Debug.LogError("Collider isn't assigned");
            integrity = false;
        }

        if(orientation == null)
        {
            Debug.LogError("Player Orientation isn't assigned");
            integrity = false;
        }

        if(player == null)
        {
            Debug.LogError("Player Transform isn't assigned");
            integrity = false;
        }

        if(physics == null)
        {
            Debug.LogError("Physics Manager isn't assigned");
            integrity = false;
        }

        return integrity;
    }

    private void OnDrawGizmos()
    {
        if(!debug) return;

        Gizmos.color = isPathBlocked ? Color.red : Color.green;
        Vector3 footPoint = player.position + Vector3.up * (EXTRA_OFFSET_RAYCAST_UP - box.bounds.extents.y);
        float footStepDistance = EXTRA_OFFSET_RAYCAST_FORWARD + box.bounds.extents.z;
        Gizmos.DrawLine(footPoint, footPoint + moveDirection * footStepDistance);

        Gizmos.color = isPathLadderStep ? Color.red : Color.green;

        float forwardDistanceRaycast = box.bounds.extents.z + EXTRA_OFFSET_RAYCAST_FORWARD;

        if(isPathBlocked)
        {
            forwardDistanceRaycast = (stepHit.point - (player.position - Vector3.up * (box.bounds.extents.y - EXTRA_OFFSET_RAYCAST_UP))).magnitude;
        }

        footPoint = player.position + Vector3.up * (stepSize + EXTRA_OFFSET_RAYCAST_UP - box.bounds.extents.y)
            + moveDirection * (forwardDistanceRaycast + .05f);

        Gizmos.DrawLine(footPoint, footPoint + Vector3.down * (stepSize + EXTRA_OFFSET_RAYCAST_UP - .05f));
    }
}
