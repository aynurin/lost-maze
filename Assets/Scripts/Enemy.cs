using UnityEngine;

public class Enemy : MovingObject {
    public int playerDamage;
    public float attacksPerSecond;
    private float attackWaitTime = 0f;
    private Animator animator;
    private Player player;
    private bool skipMove;
    private BoxCollider2D boxCollider;

    public AudioClip attack1;
    public AudioClip attack2;

    private Vector2 currentGoal = Vector2.zero;

    // Start is called before the first frame update
    protected override void Start() {
        GameManager.Instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        player = GameManager.Instance.Player;
        base.Start();
    }

    void Reset() {
        moveUnitsPerSecond = 1.5f;
        attacksPerSecond = 2f;
    }

    // Update is called once per frame
    void Update() {
        if (((Vector2)BoxCollider.bounds.center - currentGoal).sqrMagnitude < 0.2) {
            currentGoal = Vector2.zero;
        }
        if (!GameManager.Instance.IsLoading) {
            MoveEnemy();
        }
        attackWaitTime += Time.deltaTime;
    }

    public void MoveEnemy() {
        var currentCell = GameManager.Instance.mazeManager.FindCellAt(BoxCollider.bounds.center);
        var nextMazeCell = GetNextMazeCell(currentCell);
        var playerDirection = player.BoxCollider.bounds.center - BoxCollider.bounds.center;
        Vector2 direction;
        if (nextMazeCell != null) {
            if (currentGoal == Vector2.zero) {
                currentGoal = GameManager.Instance.mazeManager.FindPosition(nextMazeCell);
            }
            direction = (currentGoal - (Vector2)BoxCollider.bounds.center).normalized;
        } else {
            direction = playerDirection.sqrMagnitude > 2 ? playerDirection.normalized : playerDirection;
            currentGoal = Vector2.zero;
        }

        var mag = playerDirection.magnitude;
        if (playerDirection.sqrMagnitude > 1) {
            StartMoving(direction);
        }
    }

    private MazeCell GetNextMazeCell(MazeCell startAt) {
        DijkstraDistances dist = DijkstraDistances.Find(GameManager.Instance.mazeManager.FindCellAt(player.BoxCollider.bounds.center));
        var solution = dist.Solve(startAt);
        if (solution.HasValue && solution.Value.Count > 1) {
            return solution.Value[1];
        } else {
            return null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        currentGoal = Vector2.zero;
        var player = collision.gameObject.GetComponent<Player>();
        if (player != null) {
            Attack(player);
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        currentGoal = Vector2.zero;
        var player = collision.gameObject.GetComponent<Player>();
        if (player != null) {
            Attack(player);
        }
    }

    void Attack(Player player) {
        if (attackWaitTime >= 1f / attacksPerSecond) {
            animator.SetTrigger("EnemyAttack");
            SoundManager.Instance.RandomizeSfx(attack1, attack2);
            player.TakeDamage((float)playerDamage);
            attackWaitTime = 0f;
        }
    }
}
