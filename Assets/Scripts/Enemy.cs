using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy : MovingObject {
    public int playerDamage;
    private Animator animator;
    private Transform player;
    private bool skipMove;

    // Start is called before the first frame update
    protected override void Start() {
        GameManager.Instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        if (skipMove) {
            skipMove = false;
            return;
        }

        base.AttemptMove<T>(xDir, yDir);

        skipMove = true;
    }

    public void MoveEnemy() {

        DijkstraDistances dist = DijkstraDistances.Find(GameManager.Instance.mazeManager.FindCellAt(player.position));
        var currentCell = GameManager.Instance.mazeManager.FindCellAt(transform.position);
        var solution = dist.Solve(currentCell);
        int xDir = 0;
        int yDir = 0;
        if (solution.HasValue && solution.Value.Count > 1) {
            var needToMoveTo = solution.Value[1];

            var direction = GameManager.Instance.mazeManager.FindDirection(currentCell, needToMoveTo);
            xDir = (int)direction.x;
            yDir = (int)direction.y;
        } else {
            xDir = (int)(player.transform.position.x - transform.position.x);
            yDir = (int)(player.transform.position.y - transform.position.y);
        }

        var mag = (player.transform.position - transform.position).magnitude;
        if (mag <= 1.5) {
            Debug.Log($"Attack instead of moving with mag={mag}");
            Attack(Player.Instance);
        } else {
            Debug.Log($"Moving with mag={mag}");
            if (xDir > 0) {
                yDir = 0;
            }
            AttemptMove<Player>(xDir, yDir);
        }
    }

    protected override void OnCantMove<T>(T component) {
        Attack(component as Player);
    }

    void Attack(Player player) {
        animator.SetTrigger("EnemyAttack");
        player.LoseFood(playerDamage);
    }
}
