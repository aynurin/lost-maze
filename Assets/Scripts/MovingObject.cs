using UnityEngine;

public abstract class MovingObject : MonoBehaviour {
    public float moveUnitsPerSecond;
    public LayerMask blockingLayer;

    public BoxCollider2D BoxCollider { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    // Start is called before the first frame update
    protected virtual void Start() {
        BoxCollider = GetComponent<BoxCollider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    protected virtual void StartMoving(Vector2 direction) {
        var targetPosition = Rigidbody.position + direction * moveUnitsPerSecond * Time.fixedDeltaTime;
        Rigidbody.MovePosition(targetPosition);
    }
}
