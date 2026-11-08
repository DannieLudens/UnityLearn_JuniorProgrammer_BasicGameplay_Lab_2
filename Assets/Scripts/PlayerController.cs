using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;               // Walk speed in m/s (editable in Inspector)
    [SerializeField] private InputAction moveAction;         // Move input binding (set in Inspector)

    private Rigidbody _playerRb;                             // Cached Rigidbody for physics movement
    private Vector2 _moveInput;                              // Latest input value (x = left/right)

    private void Start()
    {
        _playerRb = GetComponent<Rigidbody>();               // Cache once, not every frame
        moveAction.Enable();                                 // Input actions start disabled
    }

    private void FixedUpdate()                               // Physics step: apply movement here
    {
        float horizontalInput = _moveInput.x;                // -1 left, 0 idle, 1 right
        Vector3 velocity = _playerRb.linearVelocity;         // Read current velocity (keeps gravity on Y)
        velocity.x = horizontalInput * speed;                // Change only the X axis
        _playerRb.linearVelocity = velocity;                 // Apply the new velocity
    }

    private void Update()                                    // Every frame: read input here
    {
        _moveInput = moveAction.ReadValue<Vector2>();        // Store input for FixedUpdate
    }
}
