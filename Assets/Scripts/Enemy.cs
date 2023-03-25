using System.Collections;
using System.Linq;
using UnityEngine;

public class Enemy : MovingObject {

    /// <summary>
    /// Player will be lost from view after a cooldown time passed after the player actually leaving the field of view
    /// </summary>
    public float canSeePlayerCooldownTime;
    public float attacksPerSecond;
    public int playerDamage;
    /// <summary>
    /// Max number of cells the enemy can move away from their base while patrolling
    /// </summary>
    public int maxPatrolDistanceFromBase;
    [Range(1, 360)] public float fovAngle;
    public float fovDepth;

    public LayerMask targetLayer;
    public LayerMask obstructionLayer;
    public AudioClip attack1;
    public AudioClip attack2;

    public bool CanSeePlayer { get; private set; }

    private readonly float inAlertMaxTime = 1f;
    private float canSeePlayerCooldownTimer = 0f;
    private float attackTimer = 0f;
    private float inAlertTimer = 0f;

    private Player player;

    private Vector2 currentGoal = Vector2.zero;
    private Vector2 currentDirection;
    private MazeCell baseCell;
    private DijkstraDistances distFromBase;

    void Reset() {
        playerDamage = 1;
        moveUnitsPerSecond = 1.5f;
        attacksPerSecond = 2f;
        maxPatrolDistanceFromBase = 3;
        fovAngle = 130;
        fovDepth = 15f;
        canSeePlayerCooldownTime = 1f;
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();
        GameManager.Instance.AddEnemyToList(this);
        CurrentState = State.Patroll;
        player = GameManager.Instance.Player;
        StartCoroutine(FovCheck());
    }

    // Update is called once per frame
    void Update() {
        if (((Vector2)BoxCollider.bounds.center - currentGoal).sqrMagnitude < 0.2) {
            currentGoal = Vector2.zero;
        }
        if (!GameManager.Instance.IsLoading) {
            MoveEnemy();
        }
        attackTimer += Time.deltaTime;
    }

    public void MoveEnemy() {
        /**
        Enemy continuously patrols an area around their base
        If enemy sees the player, they will charge
        When the sight of player is lost, the enemy will stop chasing after a cooldown time
         */
        var currentCell = GameManager.Instance.mazeManager.FindCellAt(BoxCollider.bounds.center);
        var playerDirection = (player.BoxCollider.bounds.center - BoxCollider.bounds.center);

        State currentState = CurrentState;
        State targetState;
        Vector2 moveDirection;
        Vector2 lookDirection;
        if (CanSeePlayer) {
            if (currentState != State.Charge) {
                currentGoal = Vector2.zero;
            }
            targetState = State.Charge;
            moveDirection = ChargePlayer(currentCell, playerDirection);
            lookDirection = moveDirection;
        } else {
            // the player is not visible
            if (currentState == State.Charge) {
                targetState = State.Alert;
                moveDirection = Vector2.zero;
                lookDirection = playerDirection.normalized;
                inAlertTimer = 0;
                currentGoal = Vector2.zero;
            } else if (currentState == State.Alert) {
                inAlertTimer += Time.deltaTime;
                if (inAlertTimer <= inAlertMaxTime) {
                    targetState = State.Alert;
                    moveDirection = Vector2.zero;
                    lookDirection = currentDirection;
                    currentGoal = Vector2.zero;
                } else {
                    targetState = State.Patroll;
                    moveDirection = Patrol(currentCell);
                    lookDirection = moveDirection;
                    inAlertTimer = 0;
                }
            } else {
                targetState = State.Patroll;
                moveDirection = Patrol(currentCell);
                lookDirection = moveDirection;
                inAlertTimer = 0;
            }
        }

        if (CurrentState != targetState) {
            CurrentState = targetState;
        }

        // Debug.Log($"{currentState}->{targetState}; move: {moveDirection}; look: {lookDirection}; see: {CanSeePlayer}");

        currentDirection = lookDirection;
        Animator.SetFloat("horizontal", lookDirection.x);
        Animator.SetFloat("vertical", lookDirection.y);
        if (playerDirection.sqrMagnitude > 1) {
            StartMoving(moveDirection);
        }
    }


    private State CurrentState {
        get {
            return (State)Animator.GetInteger("mode");
        }
        set {
            switch (value) {
                case State.Alert:
                case State.Idle:
                    speedMultiplier = 0;
                    break;
                case State.Patroll:
                case State.Search:
                    speedMultiplier = 0.3f;
                    break;
                default:
                    speedMultiplier = 1;
                    break;
            }
            Animator.SetInteger("mode", (int)value);
        }
    }

    private enum State {
        Idle = 0,
        Alert,
        Patroll,
        Search,
        Charge
    }

    private Vector2 Patrol(MazeCell currentCell) {
        if (currentGoal == Vector2.zero) {
            var nextCell = currentCell.Links.Where(c => distFromBase[c] <= maxPatrolDistanceFromBase).RandomOrNull();
            if (nextCell == null) {
                nextCell = currentCell.Links.OrderBy(c => distFromBase[c]).FirstOrDefault();
            }
            if (nextCell == null) {
                Debug.LogError($"No cell to patroll from {currentCell}");
            } else {
                currentGoal = GameManager.Instance.mazeManager.FindPosition(nextCell);
            }
            Debug.Log($"{currentCell}->{nextCell}: {currentGoal}");
        }
        Debug.Log($"{currentCell}: {currentGoal} ({((currentGoal - (Vector2)BoxCollider.bounds.center).normalized)})");
        return (currentGoal - (Vector2)BoxCollider.bounds.center).normalized;
    }

    private Vector2 ChargePlayer(MazeCell currentCell, Vector2 playerDirection) {
        DijkstraDistances dist = DijkstraDistances.Find(GameManager.Instance.mazeManager.FindCellAt(player.BoxCollider.bounds.center));
        var solution = dist.Solve(currentCell);
        if (solution.HasValue && solution.Value.Count > 1) {
            if (currentGoal == Vector2.zero) {
                currentGoal = GameManager.Instance.mazeManager.FindPosition(solution.Value[1]);
            }
            return (currentGoal - (Vector2)BoxCollider.bounds.center).normalized;
        } else {
            currentGoal = Vector2.zero;
            return playerDirection.sqrMagnitude > 1.0f ? playerDirection.normalized : playerDirection;
        }
    }

    private IEnumerator FovCheck() {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true) {
            yield return wait;
            var target = Physics2D.OverlapCircle(transform.position, fovDepth, targetLayer);
            var canSeePlayer = false;
            if (target) {
                float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
                Vector2 directionToTarget = (target.transform.position - transform.position).normalized;
                canSeePlayer = Vector2.Angle(currentDirection, directionToTarget) < fovAngle / 2 &&
                    !Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionLayer);
            }
            if (CanSeePlayer != canSeePlayer) {
                if (!canSeePlayer) {
                    // player lost from view will happen after a cooldown
                    if (canSeePlayerCooldownTimer > canSeePlayerCooldownTime) {
                        CanSeePlayer = canSeePlayer;
                    } else if (canSeePlayerCooldownTimer < float.Epsilon) {
                        canSeePlayerCooldownTimer = float.Epsilon * 2;
                    } else {
                        canSeePlayerCooldownTimer += 0.2f;
                    }
                } else {
                    CanSeePlayer = true;
                    canSeePlayerCooldownTimer = 0;
                }
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, fovDepth);

        Vector3 angle01 = DirectionFromAngle(-transform.eulerAngles.z, -fovAngle / 2);
        Vector3 angle02 = DirectionFromAngle(-transform.eulerAngles.z, +fovAngle / 2);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + angle01 * fovDepth);
        Gizmos.DrawLine(transform.position, transform.position + angle02 * fovDepth);

        if (CanSeePlayer) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.gameObject.transform.position);
        }
    }

    private Vector2 DirectionFromAngle(float eulerY, float angleInDegrees) {
        angleInDegrees += eulerY;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    internal void StartGameAt(MazeCell cell) {
        baseCell = cell;
        distFromBase = DijkstraDistances.Find(cell);
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
        if (attackTimer >= 1f / attacksPerSecond) {
            if (Animator.parameters.Length == 1) {
                Animator.SetTrigger("EnemyAttack");
            } else {
                Animator.SetTrigger("attack");
            }
            SoundManager.Instance.RandomizeSfx(attack1, attack2);
            player.TakeDamage((float)playerDamage);
            attackTimer = 0f;
        }
    }
}
