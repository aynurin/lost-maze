using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeRendererIsometric : MonoBehaviour, IMazeRenderer
{
    public int cellWidth = 3;
    public int cellHeight = 2;
    public int defaultWallThikness = 1;
    public TileBase wallRuleTile;

    [HideInInspector] public Tilemap tilemap;

    private MazeGrid mazeGrid;

    public void Reset() {
        cellWidth = 3;
        cellHeight = 3;
        defaultWallThikness = 1;
    }

    public void RenderScene(MazeGrid grid) {
        mazeGrid = grid;
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        CreateMaze();
    }

    public Vector3 GetSceneCenter() {
        return new Vector3(mazeGrid.Cols * cellWidth / 2, -mazeGrid.Rows * cellHeight / 2, -10);
    }

    public Vector3 GetCellCenter(MazeCell cell)
    {
        var x = cell.Col * cellWidth + cellWidth / 2;
        var y = -cell.Row * cellHeight - cellHeight / 2;
        return new Vector3(x, y, 0f);
    }

    public MazeCell GetCellAt(Vector3 position) {
        var col = (int)position.x / cellWidth;
        var row = -(int)position.y / cellHeight;
        var cell = mazeGrid[row, col];
        return cell;
    }

    public void CreateMaze() {
        // cols * colwidth + (cols - 1) * wall
        for (int col = 0; col < mazeGrid.Cols; col++) {
            for (int row = 0; row < mazeGrid.Rows; row++) {
                for (int cellX = 0; cellX < cellWidth; cellX++) {
                    for (int cellY = 0; cellY < cellHeight; cellY++) {
                        var sceneX = col * cellWidth + cellX;
                        var sceneY = -(row * cellHeight + cellY);
                        tilemap.SetTile(new Vector3Int(sceneX, sceneY, 0), wallRuleTile);
                    }
                }
            }
        }
    }
}
