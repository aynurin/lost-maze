using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager {
    private readonly MazeGrid mazeGrid;
    private DijkstraDistances distances;

    private List<MazeCell> unoccupiedCells;
    private List<MazeCell> solution;

    public MazeCell GameStartCell => solution.First();

    public MazeCell GameFinishCell => solution.Last();

    public ICollection<MazeCell> UnoccupiedCells => unoccupiedCells;

    public int SceneComplexity => (int)Math.Round(Math.Sqrt(mazeGrid.Rows * mazeGrid.Cols));

    public MazeGrid MazeGrid => mazeGrid;

    public MazeManager(int rows, int columns) {
        mazeGrid = new MazeGrid(rows, columns);
    }

    public void Setup() {
        new AldousBroderMazeGenerator().Generate(mazeGrid);
        distances = DijkstraDistances.FindLongest(mazeGrid[0, 0]);

        unoccupiedCells = new List<MazeCell>(mazeGrid.Cells);
        solution = new List<MazeCell>(distances.Solution.Value);
    }

    public void MarkCellsOccupied(MazeCell anchor, int radius = 0) {
        for (int i = unoccupiedCells.Count - 1; i >= 0; i--) {
            var cell = unoccupiedCells[i];
            if (Math.Abs(cell.Row - anchor.Row) < radius && Math.Abs(cell.Col - anchor.Col) < radius) {
                unoccupiedCells.RemoveAt(i);
            }
        }
    }
}
