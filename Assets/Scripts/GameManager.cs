using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public float playerFoodPoints = 100;
    public GameObject[] enemyObjects;
    public GameObject exitObject;
    public GameObject[] foodObjects;
    public int mazeColumns = 16;
    public int mazeRows = 8;

    [HideInInspector] internal Player Player { get => GameObject.FindGameObjectWithTag("Player").GetComponent<Player>(); }
    [HideInInspector] internal bool IsLoading { get; private set; }
    [HideInInspector] internal MazeRenderer mazeRenderer;

    private MazeManager mazeManager;
    private int level = 1;
    private float levelStartDelay = 2f;
    private Text levelText;
    private GameObject levelImage;
    private List<Enemy> enemies;

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

    void Reset() {
        mazeColumns = 16;
        mazeRows = 8;
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        mazeRenderer = GetComponent<MazeRenderer>();
        InitGame();
    }

    public void AddEnemyToList(Enemy script) {
        enemies.Add(script);
    }

    public void GameOver() {
        Debug.Log("Game over");
        levelText.text = $"After {level} number of days you starved.";
        enabled = false;
    }

    void InitGame() {
        IsLoading = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);

        //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();

        mazeManager = new MazeManager(mazeRows, mazeColumns);
        mazeManager.Setup();
        mazeRenderer.RenderScene(mazeManager.MazeGrid);

        PlacePlayer();
        PlaceEnemies();
        PlaceExit();
        PlaceFood();
    }

    private void PlacePlayer() {
        GameManager.Instance.Player.gameObject.transform.position = 
            mazeRenderer.GetCellCenter(mazeManager.GameStartCell);
        mazeManager.MarkCellsOccupied(mazeManager.GameStartCell, 3);
    }

    private void PlaceExit() {
        Instantiate(exitObject, mazeRenderer.GetCellCenter(mazeManager.GameFinishCell), Quaternion.identity);
        mazeManager.MarkCellsOccupied(mazeManager.GameFinishCell, 3);
    }

    private void PlaceEnemies() {
        var enemyCount = (int)Mathf.Log(level, 2F) + 1;
        var enemyCells = mazeManager.UnoccupiedCells.RandomItems(enemyCount);
        foreach (var cell in enemyCells) {
            var enemyTile = enemyObjects.RandomItem();
            var enemy = Instantiate(enemyTile, mazeRenderer.GetCellCenter(cell), Quaternion.identity);
            enemy.GetComponent<Enemy>().StartGameAt(cell);
            mazeManager.MarkCellsOccupied(cell, 1);
        }
    }

    private void PlaceFood() {
        int needed = mazeManager.SceneComplexity; // - currentFood;
        int maxItems = 10;
        while (needed > 0 && maxItems > 0) {
            maxItems--;
            var tile = foodObjects.RandomItem();
            if (tile.tag == "Food") {
                needed -= Player.Instance.pointsPerFood;
            } else if (tile.tag == "Soda") {
                needed -= Player.Instance.pointsPerSoda;
            }
            var cell = mazeManager.UnoccupiedCells.RandomItem();
            if (cell == null) {
                Debug.LogWarning("No place to place food...");
            } else {
                Instantiate(tile, mazeRenderer.GetCellCenter(cell), Quaternion.identity);
                mazeManager.MarkCellsOccupied(cell, 1);
            }
        }
    }

    private void HideLevelImage() {
        levelImage.SetActive(false);
        IsLoading = false;
    }
}
