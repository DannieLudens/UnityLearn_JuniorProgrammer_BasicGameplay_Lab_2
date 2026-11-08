using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;               // What to follow
    [SerializeField] private float smoothTime = 0.2f;        // Seconds to catch up (0 = instant)

    private Vector3 _offset;                                 // Camera position relative to target (player)
    private Vector3 _velocity;                               // Used internally by SmoothDamp
    
    // Remember the framing set up in the scene
    private void Start()
    {
        _offset = transform.position - target.position;      // Camera minus player = distance to keep
    }

    // Move after everything else has moved this frame
    private void LateUpdate()
    {
        Vector3 desired = target.position + _offset;         // Where the camera should be
        // desired.y = transform.position.y;                    // Keep current height (follow X only)
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime); // Ease toward target
    }
}
