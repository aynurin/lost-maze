using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    public static Player Instance { get; private set; }
    public int Food { get => food; }

    private Animator animator;
    private int food;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
    }

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoseFood(int loss) {
        animator.SetTrigger("PlayerHit");
        food -= loss;
        foodText.text = $"-{loss} Food: {food}";
        CheckIfGameOver();
    }

    protected override void Start() {
        animator = GetComponent<Animator>();
        food = GameManager.Instance.playerFoodPoints;
        foodText.text = $"Food {food}";
        base.Start();
    }

    // Update is called once per frame
    void Update() {
        if (!GameManager.Instance.playersTurn) return;

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
        GameManager.Instance.playerFoodPoints = food;
    }

    protected override void AttemptMove<T>(int xDir, int yDir) {
        food--;
        foodText.text = $"Food {food}";

        base.AttemptMove<T>(xDir, yDir);

        // RaycastHit2D hit;


        CheckIfGameOver();

        GameManager.Instance.playersTurn = false;
    }

    private void CheckIfGameOver() {
        if (food <= 0) {
            GameManager.Instance.GameOver();
        }
    }
}
