using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeRendererIsometric : MonoBehaviour, IMazeRenderer
{
    public int cellWidth = 3;
    public int cellHeight = 2;
    public int wallThikness = 1;
    public TileBase wallRuleTile;
    public TileBase debugRuleTile;

    [HideInInspector] public Tilemap tilemap;

    private MazeGrid mazeGrid;

    public void Reset() {
        cellWidth = 3;
        cellHeight = 3;
        wallThikness = 1;
    }

    public void RenderScene(MazeGrid grid) {
        mazeGrid = grid;
        tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        CreateMaze();
    }

    public Vector3 GetSceneCenter() {
        return tilemap.CellToLocal(new Vector3Int(mazeGrid.Cols * (cellWidth + wallThikness) / 2, -mazeGrid.Rows * (cellHeight + wallThikness) / 2, -10));
    }

    public Vector3 GetCellCenter(MazeCell cell) {
        var x = cell.Col * (cellWidth + wallThikness) + cellWidth / 2;
        var y = cell.Row * (cellHeight + wallThikness) + cellHeight / 2;
        return tilemap.CellToLocal(new Vector3Int(x, -y, 0));
    }

    public MazeCell GetCellAt(Vector3 position) {
        var cellPosition = tilemap.LocalToCell(position);
        var col = (int)cellPosition.x / (cellWidth + wallThikness);
        var row = -(int)cellPosition.y / (cellHeight + wallThikness);
        var cell = mazeGrid[row, col];
        return cell;
    }

    public void CreateMaze() {
        var allCells = mazeGrid.Cells;
        foreach (var cell in allCells) {
            var worldX = cell.Col * (cellWidth + wallThikness);
            var worldY = cell.Row * (cellHeight + wallThikness);
            for (int cellX = 0; cellX < cellWidth; cellX++) {
                for (int cellY = 0; cellY < cellHeight; cellY++) {
                    var sceneX = worldX + cellX;
                    var sceneY = worldY + cellY;
                    tilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), wallRuleTile);
                }
            }
            if (cell.NorthGate.HasValue) {
                for (int gateX = 0; gateX < cellWidth; gateX++) {
                    for (int gateY = 0; gateY < wallThikness; gateY++) {
                        var sceneX = worldX + gateX;
                        var sceneY = worldY - gateY - 1;
                        tilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), wallRuleTile);
                    }
                }
            }
            if (cell.SouthGate.HasValue) {
                for (int gateX = 0; gateX < cellWidth; gateX++) {
                    for (int gateY = 0; gateY < wallThikness; gateY++) {
                        var sceneX = worldX + gateX;
                        var sceneY = worldY + cellHeight + gateY;
                        tilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), wallRuleTile);
                    }
                }
            }
            if (cell.EastGate.HasValue) {
                for (int gateX = 0; gateX < wallThikness; gateX++) {
                    for (int gateY = 0; gateY < cellHeight; gateY++) {
                        var sceneX = worldX + cellWidth + gateX;
                        var sceneY = worldY + gateY;
                        tilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), wallRuleTile);
                    }
                }
            }
            if (cell.WestGate.HasValue) {
                for (int gateX = 0; gateX < wallThikness; gateX++) {
                    for (int gateY = 0; gateY < cellHeight; gateY++) {
                        var sceneX = worldX - gateX - 1;
                        var sceneY = worldY + gateY;
                        tilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), wallRuleTile);
                    }
                }
            }
        }
    }
}
