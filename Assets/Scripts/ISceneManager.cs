using UnityEngine;

public interface ISceneManager {
    void SetupScene(int level);

    Vector3 GetSceneCenter();

    Vector3 FindPosition(MazeCell cell);

    MazeCell FindCellAt(Vector3 position);
}
