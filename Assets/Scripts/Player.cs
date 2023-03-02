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

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound1;

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
            SoundManager.Instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        } else if (other.tag == "Soda") {
            food += pointsPerSoda;
            foodText.text = $"+{pointsPerSoda} Food: {food}";
            SoundManager.Instance.RandomizeSfx(drinkSound1, drinkSound2);
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

        var horizontal = (int)Input.GetAxisRaw("Horizontal");
        var vertical = (int)Input.GetAxisRaw("Vertical");

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

    protected override void AttemptMove<T>(float xDir, float yDir) {
        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;
        if (Move(xDir, yDir, out hit)) {
            food--;
            foodText.text = $"Food {food}";
            SoundManager.Instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckIfGameOver();

        GameManager.Instance.playersTurn = false;
    }

    private void CheckIfGameOver() {
        if (food <= 0) {
            SoundManager.Instance.PlaySingle(gameOverSound1);
            SoundManager.Instance.musicSource.Stop();
            GameManager.Instance.GameOver();
        }
    }
}
