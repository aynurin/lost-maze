using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour {
    public MazeGrid mazeGrid;
    public DijkstraDistances distances;
    public int columns = 10;
    public int cellWidth = 3;
    public int rows = 10;
    public int cellHeight = 2;

    public GameObject[] outerAreaTiles;
    public GameObject[] groundTiles;
    public GameObject playerTile;
    public GameObject exitTile;
    public GameObject[] enemiesTiles;
    public GameObject[] foodTiles;

    private Transform boardHolder;

    public Transform BoardHolder { get => boardHolder; }

    public void SetupScene(int level) {
        mazeGrid = new MazeGrid(rows, columns);
        new AldousBroderMazeGenerator().Generate(mazeGrid);
        distances = DijkstraDistances.FindLongest(mazeGrid[0, 0]);

        Groundwork();
        CreateMaze();

        //PlaceTile(playerTile, distances.Solution.Value.First());
        PlaceInstance(GameObject.FindGameObjectWithTag("Player"), distances.Solution.Value.First());
        PlaceTile(exitTile, distances.Solution.Value.Last());
        PlaceTiles(enemiesTiles, RandomItems(mazeGrid.Cells, (int)Mathf.Log(level, 2F)));
        PlaceTiles(foodTiles, RandomItems(distances.Solution.Value, (int)Mathf.Log(level, 2F)));
        //distances.Solution.Value[0].
    }

    private void PlaceTile(GameObject tile, MazeCell cell) {
        Instantiate(tile, FindPosition(cell), Quaternion.identity);
    }

    private void PlaceInstance(GameObject gameObject, MazeCell cell) {
        gameObject.transform.position = FindPosition(cell);
    }

    private Vector3 FindPosition(MazeCell cell) {
        var x = cell.Col * cellWidth + cellWidth / 2;
        var y = -cell.Row * cellHeight - cellHeight / 2;
        return new Vector3(x, y, 0f);
    }

    private void PlaceTiles(ICollection<GameObject> tiles, IEnumerable<MazeCell> cells) {
        foreach (var cell in cells) {
            PlaceTile(RandomItem(tiles), cell);
        }
    }

    public void Groundwork() {
        if (outerAreaTiles == null || outerAreaTiles.Length == 0 || groundTiles == null || groundTiles.Length == 0) {
            return;
        }
        for (int x = -10; x < mazeGrid.Cols * cellWidth + 10; x++) {
            for (int y = -10; y < mazeGrid.Rows * cellHeight + 10; y++) {
                GameObject tileToRender;
                if (x < 0 || y < 0 || x >= mazeGrid.Cols * cellWidth || y >= mazeGrid.Rows * cellHeight) {
                    tileToRender = RandomItem(outerAreaTiles);
                } else {
                    tileToRender = RandomItem(groundTiles);
                }
                var groundTile = (GameObject)Instantiate(tileToRender, new Vector3(x, -y, 0f), Quaternion.identity);
                groundTile.transform.SetParent(boardHolder);
            }
        }
    }

    private static T RandomItem<T>(ICollection<T> pool) {
        return pool.ElementAt(UnityEngine.Random.Range(0, pool.Count));
    }

    private IEnumerable<T> RandomItems<T>(IList<T> value, int count) {
        for (int i = 0; i < count; i++) {
            yield return RandomItem(value);
        }
    }

    private IList<T> RandomItems<T>(T[] value, int count) {
        List<T> vals = new List<T>();
        for (int i = 0; i < count; i++) {
            vals.Add(RandomItem(value));
        }
        return vals;
    }



    public void CreateMaze() {
        boardHolder = new GameObject("Board").transform;

        GameObject[] mazeTiles = Resources.LoadAll<GameObject>("Maze/Tiles");

        for (int x = -1; x < mazeGrid.Cols + 1; x++) {
            for (int y = -1; y < mazeGrid.Rows + 1; y++) {
                Dictionary<TileSide, List<Position>> cellCoords;
                if (x >= 0 && x < mazeGrid.Cols && y >= 0 && y < mazeGrid.Rows) {
                    cellCoords = GetCellCoords(mazeGrid[y, x]);
                } else {
                    cellCoords = GetEdgeCoords(x, y);
                }
                foreach (var coord in cellCoords) {
                    var tile = LookupTile(coord.Key, mazeTiles);
                    foreach (var position in coord.Value) {
                        var mazeTile = (GameObject)Instantiate(tile, new Vector3(position.x, -position.y, 0f), Quaternion.identity);
                        mazeTile.transform.SetParent(boardHolder);
                    }
                }
            }
        }
    }

    private Dictionary<TileSide, List<Position>> GetCellCoords(MazeCell cell) {
        var cellCoords = new Dictionary<TileSide, List<Position>>();
        var tileSide = TileSide.Inner;
        for (int x = cell.Col * cellWidth; x < (cell.Col + 1) * cellWidth; x++) {
            for (int y = cell.Row * cellHeight; y < (cell.Row + 1) * cellHeight; y++) {
                tileSide = TileSide.Inner;
                int sides = 0;
                int gates = 0;
                if (x == cell.Col * cellWidth) { // west edge
                    tileSide |= TileSide.West;
                    sides++;
                    if (cell.WestGate.HasValue) {
                        tileSide |= TileSide.GateWest;
                        gates++;
                    }
                } else if (x == (cell.Col + 1) * cellWidth - 1) { // east edge
                    tileSide |= TileSide.East;
                    sides++;
                    if (cell.EastGate.HasValue) {
                        tileSide |= TileSide.GateEast;
                        gates++;
                    }
                }
                if (y == cell.Row * cellHeight) { // north edge
                    tileSide |= TileSide.North;
                    sides++;
                    if (cell.NorthGate.HasValue) {
                        tileSide |= TileSide.GateNorth;
                        gates++;
                    }
                } else if (y == (cell.Row + 1) * cellHeight - 1) { // south edge
                    tileSide |= TileSide.South;
                    sides++;
                    if (cell.SouthGate.HasValue) {
                        tileSide |= TileSide.GateSouth;
                        gates++;
                    }
                }
                if (sides == 2 && gates == 1) {
                    var gate = tileSide & TileSide.AnyGate; // find only gates
                    var gateSide = (TileSide)((int)gate >> 4); // find gate sides
                    var side = tileSide ^ gate; // find sides w/o gates
                    var wallSide = side ^ gateSide; // find the wall side
                    if (UTurnRoadblocks(cell, gateSide, wallSide) > 0) {
                        // peek a side that doesn't have a gate on it
                        tileSide = wallSide;
                    }
                    // if this corner is connected to any other wall, then replace the Gate tile with a regular tile
                }
                // else {
                //     tileSide = TileSide.Inner;
                // }
                else if (sides == 2 && gates == 2) {
                    // every cell consists of at least four blocks, so every block can be only one corner with two
                    // gates. A block cannot have more than two walls or gates. And if it has two gates, then we need to
                    // use a special tile that decorates the corner.
                    var sideOne = tileSide.HasFlag(TileSide.North) ? TileSide.North : TileSide.South;
                    var sideTwo = tileSide.HasFlag(TileSide.East) ? TileSide.East : TileSide.West;
                    if (UTurnRoadblocks(cell, sideOne, sideTwo) < 2) {
                        tileSide = TileSide.Inner;
                    }
                } else if (sides == gates) {
                    tileSide = TileSide.Inner;
                }
                if (!cellCoords.TryAdd(tileSide, new List<Position>() { new int[] { x, y } })) {
                    cellCoords[tileSide].Add(new int[] { x, y });
                }
            }
        }
        return cellCoords;
    }

    private int UTurnRoadblocks(MazeCell cell, TileSide firstSide, TileSide secondSide) {
        // ensure that:
        // cell has a gate at GateSide === true
        // cell gateSide neighbour has a sideB gate == true
        // cell sideB neighbour has a gateSide gate == true

        var firstNeighbour = cell.GetNeighbour(dx(firstSide), dy(firstSide));
        var secondNeighbour = cell.GetNeighbour(dx(secondSide), dy(secondSide));
        var firstNeighbourGate = firstNeighbour.HasValue && firstNeighbour.Value.GetGate(dx(secondSide), dy(secondSide)).HasValue;
        var secondNeighbourGate = secondNeighbour.HasValue && secondNeighbour.Value.GetGate(dx(firstSide), dy(firstSide)).HasValue;

        int roadBlocks = 0;
        if (!firstNeighbourGate) {
            roadBlocks++;
        }
        if (!secondNeighbourGate) {
            roadBlocks++;
        }
        var message = $"For cell {cell.Col}x{cell.Row} the {firstSide} neighbour has a gate: {firstNeighbourGate} " +
                      $"and {secondSide} neighbour has a gate: {secondNeighbourGate}; roadblocks: {roadBlocks}";
        return roadBlocks;
    }

    private static int dx(TileSide side) {
        if (side.HasFlag(TileSide.East)) {
            return 1;
        }
        if (side.HasFlag(TileSide.West)) {
            return -1;
        }
        return 0;
    }

    private static int dy(TileSide side) {
        if (side.HasFlag(TileSide.South)) {
            return 1;
        }
        if (side.HasFlag(TileSide.North)) {
            return -1;
        }
        return 0;
    }

    private Dictionary<TileSide, List<Position>> GetEdgeCoords(int virtualX, int virtualY) {
        var cellCoords = new Dictionary<TileSide, List<Position>>();
        var tileSide = TileSide.Inner;
        int x = virtualX * cellWidth;
        int y = virtualY * cellHeight;
        if (virtualX == -1 || virtualX == mazeGrid.Cols) {
            tileSide |= TileSide.Outer;
            if (virtualX == -1) {
                tileSide |= TileSide.West;
                x = virtualX;
            } else {
                tileSide |= TileSide.East;
                x = (mazeGrid.Cols * cellWidth);
            }

            if (virtualY == -1) {
                tileSide |= TileSide.North;
                y = virtualY;
                cellCoords.Add(tileSide, new List<Position>() { new int[] { x, y } });
            } else if (virtualY == mazeGrid.Rows) {
                tileSide |= TileSide.South;
                y = mazeGrid.Rows * cellHeight;
                cellCoords.Add(tileSide, new List<Position>() { new int[] { x, y } });
            } else {
                var positions = new List<Position>();
                for (y = virtualY * cellHeight; y < (virtualY + 1) * cellHeight; y++) {
                    positions.Add(new int[] { x, y });
                }
                cellCoords.Add(tileSide, positions);
            }
        } else if (virtualY == -1 || virtualY == mazeGrid.Rows) {
            tileSide |= TileSide.Outer;
            if (virtualY == -1) {
                tileSide |= TileSide.North;
                y = virtualY;
            } else {
                tileSide |= TileSide.South;
                y = (mazeGrid.Rows * cellHeight);
            }

            var positions = new List<Position>();
            for (x = virtualX * cellWidth; x < (virtualX + 1) * cellWidth; x++) {
                positions.Add(new int[] { x, y });
            }
            cellCoords.Add(tileSide, positions);
        } else {
            throw new InvalidOperationException($"Perhaps you want to use GetCellCoords(MazeCell) for {virtualX}x{virtualY} coord");
        }
        return cellCoords;
    }

    private GameObject LookupTile(TileSide type, GameObject[] allTiles) {
        List<String> nameParts = new List<string>() { "Maze" };

        if (type.HasFlag(TileSide.Outer)) {
            nameParts.Add("Outer");
        } else {
            nameParts.Add("Inner");
        }

        StringBuilder walls = new StringBuilder();
        if (type.HasFlag(TileSide.South)) {
            walls.Append("S");
        }
        if (type.HasFlag(TileSide.North)) {
            walls.Append("N");
        }
        if (type.HasFlag(TileSide.East)) {
            walls.Append("E");
        }
        if (type.HasFlag(TileSide.West)) {
            walls.Append("W");
        }

        if (walls.Length > 0) {
            nameParts.Add(walls.ToString());
        }

        StringBuilder gates = new StringBuilder();
        if (type.HasFlag(TileSide.GateSouth)) {
            gates.Append("S");
        }
        if (type.HasFlag(TileSide.GateNorth)) {
            gates.Append("N");
        }
        if (type.HasFlag(TileSide.GateEast)) {
            gates.Append("E");
        }
        if (type.HasFlag(TileSide.GateWest)) {
            gates.Append("W");
        }

        if (gates.Length > 0) {
            nameParts.Add("G" + gates.ToString());
        }

        var prefabName = String.Join("_", nameParts);

        GameObject tile = allTiles.Where((go, i) => go.name == prefabName).FirstOrDefault();
        var found = tile != null;

        if (!found) {
            Debug.Log($"tile: {prefabName} not found");
        }

        return found ? tile : allTiles[0];
    }

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private struct Position {
        public int x;
        public int y;

        public static implicit operator Position(int[] coords) {
            return new Position() { x = coords[0], y = coords[1] };
        }
    }

    [Flags]
    private enum TileSide {
        Outer = 0b000000000000001,
        Inner = 0b000000000000000,
        North = 0b000000000000100,
        East = 0b000000000001000,
        West = 0b000000000010000,
        South = 0b000000000100000,
        GateNorth = 0b000000001000000,
        GateEast = 0b000000010000000,
        GateWest = 0b000000100000000,
        GateSouth = 0b000001000000000,
        AnyGate = GateNorth | GateEast | GateWest | GateSouth,
    }
}
