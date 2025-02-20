using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    [Header("Physics Component")]
    [SerializeField] private Transform player;
    [SerializeField] private Collider box;

    [Header("Ground, Slope and Ladder Check")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private bool onGround;
    public bool OnGround => onGround;
    [SerializeField] private float lastGroundTime;
    public float LastGroundTime => lastGroundTime;
    [SerializeField] private RaycastHit groundHit;
    public RaycastHit GroundHit => groundHit;
    [SerializeField] private bool onSlope;
    [SerializeField] private bool previousOnSlope;
    [SerializeField] private RaycastHit slopeHit;
    public bool OnSlope => onSlope;
    public bool PreviousOnSlope => previousOnSlope;
    public RaycastHit SlopeHit => slopeHit;
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private bool exitingSlope;
    public bool ExitingSlope { get { return exitingSlope; } set { exitingSlope = value; } }
    [SerializeField] private bool blockedHead;
    public bool BlockedHead => blockedHead;
    [SerializeField] private float startYScale;

    private void Awake()
    {
        if(!CheckComponents()) return;

        startYScale = player.lossyScale.y;
    }

    private void Update()
    {
        if(!CheckComponents()) return;

        UpdateLastGroundTime();
    }

    private void FixedUpdate()
    {
        if(!CheckComponents()) return;

        GroundCheck();
        CheckBlockedHead();
        SlopeCheck();
    }

    private void GroundCheck()
    {
        onGround = Physics.Raycast(player.position, Vector3.down, out groundHit, .1f + box.bounds.extents.y, whatIsGround);
    }

    private void CheckBlockedHead()
    {
        float extensionLength = box.bounds.extents.y + box.bounds.size.y;

        blockedHead = Physics.BoxCast(player.position, new Vector3(box.bounds.extents.x, .01f, box.bounds.extents.z), Vector3.up, Quaternion.identity, extensionLength, whatIsGround);
    }

    private void UpdateLastGroundTime()
    {
        if(!onGround) return;

        lastGroundTime = Time.time;
    }

    private void SlopeCheck()
    {
        previousOnSlope = onSlope;

        if(Physics.Raycast(player.position, Vector3.down, out slopeHit, .15f + box.bounds.extents.y, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            onSlope = angle != 0 && angle <= maxSlopeAngle;
        }
        else onSlope = false;
    }

    public Vector3 ProjectOnSlopePlane(Vector3 vector)
    {
        if(!onSlope) return vector;

        return Vector3.ProjectOnPlane(vector, slopeHit.normal);
    }

    private bool CheckComponents()
    {
        bool integrity = true;

        if(player == null)
        {
            Debug.LogError("Player Transform isn't assigned");
            integrity = false;
        }

        if(box == null)
        {
            Debug.LogError("Collider isn't assigned");
            integrity = false;
        }

        return integrity;
    }
}
