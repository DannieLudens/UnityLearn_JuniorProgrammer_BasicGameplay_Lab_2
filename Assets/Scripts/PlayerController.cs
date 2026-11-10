using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Walk speed in m/s (editable in Inspector)
    [SerializeField] private float jumpForce = 7f; // Impulse (N·s); with mass 1 = 7 m/s upward
    [SerializeField] private float groundCheckDistance = 1.1f; // Ray length: half collider height + 0.1
    [SerializeField] private LayerMask groundLayer; // Only these layers count as ground
    [SerializeField] private float fallMultiplier = 2.5f; // Gravity multiplier while falling
    [SerializeField] private float lowJumpMultiplier = 2f; // Gravity multiplier when jump released early

    private InputAction _moveAction; // Move input binding (set in Inspector)
    private InputAction _jumpAction; // Jump input binding (set in Inspector)
    private Rigidbody _playerRb;                    // Cached Rigidbody for physics movement
    private Vector2 _moveInput;                     // Latest input value (x = left/right)
    private bool _jumpRequested;                    // Set in Update, consumed in FixedUpdate
    private bool _controlEnabled = true;            // False during intro/cutscenes

    // Runs once before the first frame: setup
    private void Start()
    {
        _playerRb = GetComponent<Rigidbody>();      // Cache once, not every frame
        _moveAction = InputSystem.actions.FindAction("Player/Move");                        // Input actions start disabled
        _jumpAction = InputSystem.actions.FindAction("Player/Jump");                        // Input actions start disabled
    }

    // Physics step: apply movement here
    private void FixedUpdate()
    {
        MoveHorizontal();                           // Move the player based on input
        HandleJump();                               // Jump if requested in Update
        ApplyJumpGravity();                         // Apply extra gravity for better jump feel
    }

    // Every frame: read input here
    private void Update()
    {
        if (!_controlEnabled)                          // Control off: ignore input
        {
            _moveInput = Vector2.zero;                 // Stop walking (no stale input)
            return;                                    // Skip the rest of Update
        }

        _moveInput = _moveAction.ReadValue<Vector2>(); // Store input for FixedUpdate
        if (_jumpAction.WasPressedThisFrame())         // Key went down this frame, not held
        {
            _jumpRequested = true;                    // Set a flag to jump in FixedUpdate
        }
    }

    // Lets other scripts turn player input on or off
    public void SetControlEnabled(bool isEnabled)
    {
        _controlEnabled = isEnabled;                         // Store the new state
    }

    // Set X velocity from input, keep Y untouched
    private void MoveHorizontal()
    {
        float horizontalInput = _moveInput.x;        // -1 left, 0 idle, 1 right
        Vector3 velocity = _playerRb.linearVelocity; // Read current velocity (keeps gravity on Y)
        velocity.x = horizontalInput * speed;        // Change only the X axis
        _playerRb.linearVelocity = velocity;         // Apply the new velocity
    }

    // Jump once if requested and grounded, then clear the request
    private void HandleJump()
    {
        if (_jumpRequested && IsGrounded()) // Jump requested in Update?
        {
            _playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Instant upward force
        }
        _jumpRequested = false; // Always clear: midair press is discarded
    }

    // Faster fall, and shorter jump if the key is released early
    private void ApplyJumpGravity()
    {
        if (_playerRb.linearVelocity.y < 0f) // Moving down = falling
        {
            _playerRb.AddForce(Physics.gravity * (fallMultiplier - 1f),
                ForceMode.Acceleration); // Extra gravity while falling
        }

        else if (_playerRb.linearVelocity.y > 0f && !_jumpAction.IsPressed()) // Rising but jump released
        {
            _playerRb.AddForce(Physics.gravity * (lowJumpMultiplier - 1f), ForceMode.Acceleration); // Cut the jump short
        }
    }

    // True if standing on ground, false if in the air
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance,
            groundLayer); // From here, down, this far, ground layer only
    }

    // Editor-only drawing, also visible outside Play mode
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Ray color in Scene view
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance); // Same ray as IsGrounded()
    }
}
