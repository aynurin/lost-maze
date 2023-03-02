using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public float turnDelay = .1f;
    public int playerFoodPoints = 100;
    public MazeManager mazeManager;

    [HideInInspector] internal bool playersTurn = true;

    private int level = 1;
    private float levelStartDelay = 2f;
    private bool doingSetup;
    private bool enemiesMoving;
    private Text levelText;
    private GameObject levelImage;
    private List<Enemy> enemies;

    public GameObject Player { get => GameObject.FindGameObjectWithTag("Player"); }

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        mazeManager = GetComponent<MazeManager>();
        InitGame();
    }

    //this is called only once, and the paramter tell it to be called only after the scene was loaded
    //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization() {
        //register the callback to be called everytime the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //This is called each time a scene is loaded.
    static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {
        Instance.level++;
        Instance.InitGame();
    }


    public void GameOver() {
        Debug.Log("Game over");
        levelText.text = $"After {level} number of days you starved.";
        enabled = false;
    }

    void InitGame() {
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);

        //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
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
