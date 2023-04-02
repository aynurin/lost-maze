using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMazeRenderer
{
    // Start is called before the first frame update
    void RenderScene(MazeGrid grid);
    Vector3 GetSceneCenter();
    Vector3 GetCellCenter(MazeCell cell);
    MazeCell GetCellAt(Vector3 position);
}
