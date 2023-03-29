using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {
    public GameObject gameManager;
    // Start is called before the first frame update
    void Awake() {
        if (GameManager.Instance == null) {
            var gmo = Instantiate(gameManager);
        }
    }
}
