using UnityEngine;

public abstract class MovingObject : MonoBehaviour {
    public float moveUnitsPerSecond;
    protected float speedMultiplier = 1f;

    public BoxCollider2D BoxCollider { get; private set; }
    protected Rigidbody2D Rigidbody { get; private set; }
    protected Animator Animator { get; private set; }

    // Start is called before the first frame update
    protected virtual void Start() {
        BoxCollider = GetComponent<BoxCollider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }

    protected virtual void StartMoving(Vector2 direction) {
        var targetPosition = Rigidbody.position + direction * moveUnitsPerSecond * speedMultiplier * Time.fixedDeltaTime;
        var velocity = direction * moveUnitsPerSecond * speedMultiplier * Time.fixedDeltaTime;
        if (this.GetType().Name == "Player") {
            Debug.Log($"Start moving velocity: {velocity} ({direction}, {moveUnitsPerSecond}, {speedMultiplier}, {Time.fixedDeltaTime})");
        }
        // Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
        // Vector2 refVelocity = Vector2.zero;
        // Vector2.SmoothDamp(Rigidbody.velocity, velocity, ref refVelocity)
        Rigidbody.velocity = direction * moveUnitsPerSecond;
        // Rigidbody.MovePosition(targetPosition);
    }
}
