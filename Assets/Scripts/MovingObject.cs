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
        Rigidbody.velocity = direction * moveUnitsPerSecond;
    }
}
