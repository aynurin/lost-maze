using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazeRenderer : MonoBehaviour
{
    public int cellWidth = 3;
    public int cellHeight = 2;
    public int wallThikness = 1;
    public TileBase groundRuleTile;
    public TileBase wallRuleTile;
    public TileBase overlayRuleTile;
    public TileBase debugRuleTile;

    [HideInInspector] private Tilemap groundTilemap;
    [HideInInspector] private Tilemap wallsTilemap;
    [HideInInspector] private Tilemap overlayTilemap;

    private MazeGrid mazeGrid;

    public void Reset() {
        cellWidth = 3;
        cellHeight = 3;
        wallThikness = 1;
    }

    public void RenderScene(MazeGrid grid) {
        mazeGrid = grid;
        groundTilemap = GameObject.Find("GroundTilemap").GetComponent<Tilemap>();
        wallsTilemap = GameObject.Find("WallsTilemap").GetComponent<Tilemap>();
        overlayTilemap = GameObject.Find("OverlayTilemap").GetComponent<Tilemap>();
        CreateSurroundings();
        CreateMaze();
    }

    public void CreateSurroundings() {
        for (var sceneX = -wallThikness; sceneX < mazeGrid.Cols * (cellWidth + wallThikness); sceneX++) {
            for (var sceneY = -wallThikness; sceneY < mazeGrid.Rows * (cellHeight + wallThikness); sceneY++) {
                if (sceneX >= 0 && sceneX < mazeGrid.Cols * cellWidth + (mazeGrid.Cols - 1) * wallThikness && sceneY >= 0 && sceneY < mazeGrid.Rows * cellWidth + (mazeGrid.Rows - 1) * wallThikness) continue;
                var scenePosition = new Vector3Int(sceneX, -sceneY, 0);
                wallsTilemap.SetTile(scenePosition, wallRuleTile);
                overlayTilemap.SetTile(scenePosition, overlayRuleTile);
            }
        }
    }

    public void CreateMaze() {
        foreach (var cell in mazeGrid.Cells) {
            var worldX = cell.Col * (cellWidth + wallThikness);
            var worldY = cell.Row * (cellHeight + wallThikness);
            for (int cellX = 0; cellX < cellWidth; cellX++) {
                for (int cellY = 0; cellY < cellHeight; cellY++) {
                    var sceneX = worldX + cellX;
                    var sceneY = worldY + cellY;
                    groundTilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), groundRuleTile);
                }
            }
            for (int gateX = 0; gateX < cellWidth; gateX++) {
                for (int gateY = 0; gateY < wallThikness; gateY++) {
                    var sceneX = worldX + gateX;
                    var sceneY = worldY + cellHeight + gateY;
                    var scenePosition = new Vector3Int(sceneX, -sceneY, 0);
                    if (cell.SouthGate.HasValue) {
                        groundTilemap.SetTile(scenePosition, groundRuleTile);
                    } else if (cell.Row + 1 < mazeGrid.Rows) {
                        wallsTilemap.SetTile(scenePosition, wallRuleTile);
                        overlayTilemap.SetTile(scenePosition, overlayRuleTile);
                    }
                }
            }
            for (int gateX = 0; gateX < wallThikness; gateX++) {
                for (int gateY = 0; gateY < cellHeight; gateY++) {
                    var sceneX = worldX + cellWidth + gateX;
                    var sceneY = worldY + gateY;
                    var scenePosition = new Vector3Int(sceneX, -sceneY, 0);
                    if (cell.EastGate.HasValue) {
                        groundTilemap.SetTile(new Vector3Int(sceneX, -sceneY, 0), groundRuleTile);
                    } else if (cell.Col + 1 < mazeGrid.Cols) {
                        wallsTilemap.SetTile(scenePosition, wallRuleTile);
                        overlayTilemap.SetTile(scenePosition, overlayRuleTile);
                    }
                }
            }
            if (cell.Row != mazeGrid.Rows - 1 && cell.Col != mazeGrid.Cols - 1) {
                var southEastWallsCount = 0;
                if (!cell.SouthGate.HasValue) {
                    southEastWallsCount++;
                }
                if (!cell.EastGate.HasValue) {
                    southEastWallsCount++;
                }
                if (!(cell.EastNeighbour.HasValue && cell.EastNeighbour.Value.SouthGate.HasValue)) {
                    southEastWallsCount++;
                }
                if (!(cell.SouthNeighbour.HasValue && cell.SouthNeighbour.Value.EastGate.HasValue)) {
                    southEastWallsCount++;
                }
                for (var buttX = 0; buttX < wallThikness; buttX++) {
                    for (int buttY = 0; buttY < wallThikness; buttY++) {
                        var sceneX = worldX + cellWidth + buttX;
                        var sceneY = worldY + cellHeight + buttY;
                        var scenePosition = new Vector3Int(sceneX, -sceneY, 0);
                        if (southEastWallsCount <= 1) {
                            groundTilemap.SetTile(scenePosition, groundRuleTile);
                        } else if (cell.Col + 1 < mazeGrid.Cols) {
                            wallsTilemap.SetTile(scenePosition, wallRuleTile);
                            overlayTilemap.SetTile(scenePosition, overlayRuleTile);
                        }
                    }
                }
            }
        }
    }

    public Vector3 GetCellCenter(MazeCell cell) {
        var x = cell.Col * (cellWidth + wallThikness) + cellWidth / 2;
        var y = cell.Row * (cellHeight + wallThikness) + cellHeight / 2;
        return groundTilemap.GetCellCenterWorld(new Vector3Int(x, -y, 0));
        //return groundTilemap.CellToLocal(new Vector3Int(x, -y, 0));
    }

    public MazeCell GetCellAt(Vector3 position) {
        var cellPosition = groundTilemap.LocalToCell(position);
        var col = (int)cellPosition.x / (cellWidth + wallThikness);
        var row = -(int)cellPosition.y / (cellHeight + wallThikness);
        var cell = mazeGrid[row, col];
        return cell;
    }

    public Vector3 TranslateMovement(Vector3 movement) {
        var grid = groundTilemap.gameObject.GetComponentInParent<Grid>();
        return Vector3.Scale(movement, grid.cellSize);
    }
}
