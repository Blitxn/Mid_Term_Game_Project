using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Checkpoint")]
    [SerializeField] private Transform startingPoint;

    [Header("Safety")]
    [SerializeField] private float fallLimitY = -20f;

    [Header("Moving Platform")]
    [SerializeField] private bool followMovingPlatform = true;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector3 movement;
    private bool jumpRequested;

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;

    private bool canMove = true;

    // Moving platform tracking
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        capsule.isTrigger = false;
    }

    private void Start()
    {
        if (startingPoint != null)
        {
            checkpointPosition = startingPoint.position;
            checkpointRotation = startingPoint.rotation;
        }
        else
        {
            checkpointPosition = transform.position;
            checkpointRotation = transform.rotation;
        }

        Debug.Log("[PlayerController] Neko is ready!");
    }

    private void Update()
    {
        if (!canMove) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        movement = new Vector3(h, 0f, v).normalized;

        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }

        if (transform.position.y < fallLimitY)
        {
            Respawn();
        }
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        // Get current velocity
        Vector3 velocity = rb.linearVelocity;

        // Handle jumping
        if (jumpRequested && IsGrounded())
        {
            currentPlatform = null;

            velocity.y = jumpForce;
        }

        jumpRequested = false;

        // Follow moving platform
        if (followMovingPlatform && currentPlatform != null)
        {
            Vector3 platformMovement =
                currentPlatform.position - lastPlatformPosition;

            rb.position += platformMovement;

            Quaternion platformRotation =
                currentPlatform.rotation * Quaternion.Inverse(lastPlatformRotation);

            Vector3 relativePosition =
                rb.position - currentPlatform.position;

            rb.position =
                currentPlatform.position +
                platformRotation * relativePosition;

            lastPlatformPosition = currentPlatform.position;
            lastPlatformRotation = currentPlatform.rotation;
        }

        // Movement relative to Neko's rotation
        Vector3 moveDirection =
            (transform.forward * movement.z +
             transform.right * movement.x).normalized;

        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            velocity.y,
            moveDirection.z * moveSpeed
        );
    }

    private bool IsGrounded()
    {
        Vector3 origin = capsule.bounds.center;
        float distance = capsule.bounds.extents.y + groundCheckDistance;

        return Physics.Raycast(
            origin,
            Vector3.down,
            distance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Detect when Neko lands on a platform
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                currentPlatform = collision.transform;

                lastPlatformPosition = currentPlatform.position;
                lastPlatformRotation = currentPlatform.rotation;

                Debug.Log("[PlayerController] Neko landed on a platform!");

                break;
            }
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        // Keep tracking the platform while standing on it
        if (currentPlatform == collision.transform)
        {
            bool standingOnPlatform = false;

            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    standingOnPlatform = true;
                    break;
                }
            }

            if (!standingOnPlatform)
            {
                currentPlatform = null;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (currentPlatform == collision.transform)
        {
            currentPlatform = null;
        }
    }

    public void Respawn()
    {
        movement = Vector3.zero;
        jumpRequested = false;

        currentPlatform = null;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = checkpointPosition;
        rb.rotation = checkpointRotation;

        Debug.Log("[PlayerController] Neko respawned!");
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null) return;

        checkpointPosition = checkpoint.position;
        checkpointRotation = checkpoint.rotation;

        Debug.Log("[PlayerController] New checkpoint saved!");
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        movement = Vector3.zero;
        jumpRequested = false;

        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Collect Orb
        if (other.CompareTag("Orb"))
        {
            Destroy(other.gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Collect();
            }
        }

        // Trigger-based hazard
        if (other.CompareTag("Hazard"))
        {
            Respawn();
        }
    }
}
