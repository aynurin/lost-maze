using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject {
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;

    private Animator animator;
    private int food;

    protected override void OnCantMove<T>(T component) {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("PlayerChop");
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Exit") {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        } else if (other.tag == "Food") {
            food += pointsPerFood;
            foodText.text = $"+{pointsPerFood} Food: {food}";
            other.gameObject.SetActive(false);
        } else if (other.tag == "Soda") {
            food += pointsPerSoda;
            foodText.text = $"+{pointsPerSoda} Food: {food}";
            other.gameObject.SetActive(false);
        }
    }

    private void Restart() {
        //nityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        Application.LoadLevel(Application.loadedLevel);
    }

    public void LoseFood(int loss) {
        animator.SetTrigger("PlayerHit");
        food -= loss;
        foodText.text = $"-{loss} Food: {food}";
        CheckIfGameOver();
    }

    protected override void Start() {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        foodText.text = $"Food {food}";
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0) {
            vertical = 0;
        }

        if (horizontal != 0 || vertical != 0) {
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    private void OnDisable() {
        GameManager.instance.playerFoodPoints = food;
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        food--;
        foodText.text = $"Food {food}";

        Debug.Log($"{this.GetType().Name}.AttemptMove<{typeof(T).Name}>(xDir={xDir}, yDir={yDir})");
        base.AttemptMove<T>(xDir, yDir);

        // RaycastHit2D hit;

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    private void CheckIfGameOver() {
        if (food <= 0) {
            GameManager.instance.GameOver();
        }
    }
}
