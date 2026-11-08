using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;               // Walk speed in m/s (editable in Inspector)
    [SerializeField] private InputAction moveAction;         // Move input binding (set in Inspector)
    [SerializeField] private InputAction jumpAction;         // Jump input binding (set in Inspector)
    [SerializeField] private float jumpForce = 7f;           // Impulse (N·s); with mass 1 = 7 m/s upward
    [SerializeField] private float groundCheckDistance = 1.1f; // Ray length: half collider height + 0.1
    [SerializeField] private LayerMask groundLayer;          // Only these layers count as ground

    private Rigidbody _playerRb;                             // Cached Rigidbody for physics movement
    private Vector2 _moveInput;                              // Latest input value (x = left/right)
    private bool _jumpRequested;                             // Set in Update, consumed in FixedUpdate


    private void Start()
    {
        _playerRb = GetComponent<Rigidbody>();               // Cache once, not every frame
        moveAction.Enable();                                 // Input actions start disabled
        jumpAction.Enable();                                 // Same for the jump action
    }

    private void FixedUpdate()                               // Physics step: apply movement here
    {
        float horizontalInput = _moveInput.x;                // -1 left, 0 idle, 1 right
        Vector3 velocity = _playerRb.linearVelocity;         // Read current velocity (keeps gravity on Y)
        velocity.x = horizontalInput * speed;                // Change only the X axis
        _playerRb.linearVelocity = velocity;                 // Apply the new velocity

        if (_jumpRequested && IsGrounded())                  // Jump requested in Update?
        {
            _playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Instant upward force
        }
        _jumpRequested = false;                              // Always clear: midair press is discarded
    }

    private void Update()
    {
        _moveInput = moveAction.ReadValue<Vector2>();        // Store input for FixedUpdate
        if (jumpAction.WasPressedThisFrame())                // Key went down this frame, not held
        {
            _jumpRequested = true;                           // Set a flag to jump in FixedUpdate
        }
    }

    private bool IsGrounded()                                // true if standing on ground, false if in air
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer); // From here, down, this far, ground layer only
    }

    private void OnDrawGizmos()                              // Editor-only drawing, also outside Play mode
    {
        Gizmos.color = Color.red;                            // Ray color in Scene view
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance); // Same ray as IsGrounded()
    }
}
