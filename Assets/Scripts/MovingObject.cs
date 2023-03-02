using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {
    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    new private Rigidbody2D rigidbody2D;
    private float inverseMoveTime;

    // Start is called before the first frame update
    protected virtual void Start() {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    protected bool Move(float xDir, float yDir, out RaycastHit2D hit) {
        Vector2 direction = new Vector2(xDir, yDir);

        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + direction;

        Vector2 objectSize = (Vector2)(this.transform.GetComponent<Renderer>().bounds.size);
        Vector2 endBodyEdge = endPosition + (objectSize / 2 * direction.normalized);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(startPosition, endBodyEdge, blockingLayer);
        boxCollider.enabled = true;

        if (this.GetType().Name == "Player") {
            Debug.Log($"Move linecast {startPosition} to {endBodyEdge} ({objectSize}, {direction.normalized}), is collision? {hit.transform != null}");
        }

        if (hit.transform == null) {
            StartCoroutine(SmoothMovement(endPosition));
            return true;
        }

        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end) {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon) {
            Vector3 newPosition = Vector3.MoveTowards(rigidbody2D.position, end, inverseMoveTime * Time.deltaTime);
            rigidbody2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    protected virtual void AttemptMove<T>(float xDir, float yDir)
        where T : Component {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null) {
            return;
        }

        T hitComponent = hit.transform.GetComponent<T>();

        if (!canMove && hitComponent != null) {
            OnCantMove(hitComponent);
        }
    }

    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
