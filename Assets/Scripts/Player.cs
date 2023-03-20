using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {
    public static Player Instance { get; private set; }

    public int wallDamage;
    public int pointsPerFood;
    public int pointsPerSoda;
    public float restartLevelDelay;
    public Text foodText;
    public float attacksPerSecond;

    public float Food { get; private set; }

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound1;

    private Animator animator;
    private Vector2 movement;
    private float attackWaitTime = 0f;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    void Reset() {
        wallDamage = 1;
        pointsPerFood = 10;
        pointsPerSoda = 20;
        restartLevelDelay = 2f;
        attacksPerSecond = 6F;
        moveUnitsPerSecond = 3;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        var wall = collision.gameObject.GetComponent<Wall>();
        if (wall != null && collision.otherCollider.enabled) {
            attackWaitTime = 0f; // don't start breaking the wall immediately
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        var wall = collision.gameObject.GetComponent<Wall>();
        if (wall != null && collision.otherCollider.enabled) {
            CollideWithWall(wall, collision);
        }
    }

    private void CollideWithWall(Wall wall, Collision2D collision) {
        var contacts = new List<ContactPoint2D>();
        collision.GetContacts(contacts);
        var angle = contacts.Select(c => Vector2.Angle(collision.relativeVelocity, c.normal)).Average();
        if (angle < 45) {
            if (attackWaitTime >= 1f / attacksPerSecond) {
                var power = Mathf.Abs(Mathf.Cos(Mathf.PI / 180f * angle));
                ChangeFood(f => f - wallDamage * power);
                wall.DamageWall(wallDamage * power);
                animator.SetTrigger("PlayerChop");
                attackWaitTime = 0f;
            }
        } else {
            var opposingForce = contacts.Select(c => c.normal).OrderByDescending(n => n.sqrMagnitude).First();
            this.Rigidbody.AddForce(opposingForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Exit") {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        } else if (other.tag == "Food") {
            ChangeFood(f => f + pointsPerFood);
            SoundManager.Instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        } else if (other.tag == "Soda") {
            ChangeFood(f => f + pointsPerSoda);
            SoundManager.Instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }

    protected override void Start() {
        animator = GetComponent<Animator>();
        ChangeFood(f => GameManager.Instance.playerFoodPoints);
        base.Start();
    }

    private void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Update is called once per frame
    void Update() {
        // movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        if (movement.sqrMagnitude > float.Epsilon) {
            Rigidbody.velocity = Vector2.zero;
        }
    }

    void FixedUpdate() {
        attackWaitTime += Time.fixedDeltaTime;
        if (movement.magnitude > float.Epsilon) {
            StartMoving(movement.sqrMagnitude > 1 ? movement.normalized : movement);
        }
    }

    private void ChangeFood(Func<float, float> change, string prefix = "") {
        Food = change(Food);
        foodText.text = (String.IsNullOrEmpty(prefix) ? "" : prefix) + "Food: " + (int)Math.Round(Food);
        CheckIfGameOver();
    }

    public void TakeDamage(float damage) {
        animator.SetTrigger("PlayerHit");
        ChangeFood(f => f - damage, $"-{damage} ");
    }

    private void OnDisable() {
        if (GameManager.Instance == null) {
            Debug.Log("GameManager not found, probably the game is being destroyed...");
            return;
        }
        GameManager.Instance.playerFoodPoints = Food;
    }

    protected override void StartMoving(Vector2 direction) {
        base.StartMoving(direction);
        SoundManager.Instance.RandomizeSfx(moveSound1, moveSound2);
    }

    private void CheckIfGameOver() {
        if (Food <= float.Epsilon) {
            SoundManager.Instance.PlaySingle(gameOverSound1);
            SoundManager.Instance.musicSource.Stop();
            GameManager.Instance.GameOver();
        }
    }
}
