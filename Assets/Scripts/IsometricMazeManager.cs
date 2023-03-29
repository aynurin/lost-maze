using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IsometricMazeManager : MonoBehaviour, ISceneManager {
    public MazeGrid mazeGrid;
    public DijkstraDistances distances;
    public int columns = 10;
    public int cellWidth = 3;
    public int rows = 10;
    public int cellHeight = 2;

    public Tilemap tilemap;

    public TileBase[] wallTiles;
    public TileBase[] groundTiles;
    public GameObject playerTile;
    public GameObject exitTile;
    public GameObject[] enemiesTiles;
    public GameObject[] foodTiles;

    private Transform boardHolder;

    public void SetupScene(int level) {
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        mazeGrid = new MazeGrid(rows, columns);
        new AldousBroderMazeGenerator().Generate(mazeGrid);
        distances = DijkstraDistances.FindLongest(mazeGrid[0, 0]);

        var allCells = new List<MazeCell>(mazeGrid.Cells);
        var solution = new List<MazeCell>(distances.Solution.Value);

        CreateMaze();
    }

    public Vector3 FindPosition(MazeCell cell) {
        var x = cell.Col * cellWidth + cellWidth / 2;
        var y = -cell.Row * cellHeight - cellHeight / 2;
        return new Vector3(x, y, 0f);
    }

    public MazeCell FindCellAt(Vector3 position) {
        var col = (int)position.x / cellWidth;
        var row = -(int)position.y / cellHeight;
        var cell = mazeGrid[row, col];
        return cell;
    }

    public void CreateMaze() {

        GameObject[] mazeTiles = Resources.LoadAll<GameObject>("Maze/Tiles");

        for (int x = -1; x < mazeGrid.Cols + 1; x++) {
            for (int y = -1; y < mazeGrid.Rows + 1; y++) {
                tilemap.SetTile(new Vector3Int(x, -y, 0), groundTiles[0]);
            }
        }
    }

    public Vector3 GetSceneCenter() {
        return new Vector3(columns * cellWidth / 2, -rows * cellHeight / 2, -10);
    }
}
