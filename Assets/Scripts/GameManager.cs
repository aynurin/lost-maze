using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public float turnDelay = .1f;
    public static GameManager instance = null;
    public BoardManager boardManager;
    public MazeManager mazeManager;

    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    private float levelStartDelay = 2f;
    private UnityEngine.UI.Text levelText;
    private GameObject levelImage;
    private bool doingSetup;
    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;

    void Awake() {
        Debug.Log("GameManager.Awake");
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardManager = GetComponent<BoardManager>();
        mazeManager = GetComponent<MazeManager>();
        InitGame();
    }

    private void OnLevelWasLoaded(int index) {
        level++;
        InitGame();
    }

    public void GameOver() {
        levelText.text = $"After {level} number of days you starved.";
        enabled = false;
    }

    void InitGame() {
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<UnityEngine.UI.Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);

        //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardManager.SetupScene(level);
        mazeManager.SetupScene(level);
        CenterCamera();
    }

    private void HideLevelImage() {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    void CenterCamera() {
        var mazeCenter = new Vector3(mazeManager.columns * mazeManager.cellWidth / 2, -mazeManager.rows * mazeManager.cellHeight / 2, -10);
        GameObject.Find("Main Camera").transform.position = mazeCenter;
    }

    IEnumerator MoveEnemies() {
        Debug.Log("MoveEnemies.Start");
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0) {
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++) {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
        Debug.Log("MoveEnemies.End");
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (playersTurn || enemiesMoving || doingSetup) {
            return;
        }

        StartCoroutine(MoveEnemies());
    }

    public void AddEnemyToList(Enemy script) {
        enemies.Add(script);
    }
}
