using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
    public GameObject indicator;
    public bool isLastCheckpoint = false;

    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update() {
        indicator.SetActive(isLastCheckpoint);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            Debug.LogFormat("Checkpoint was hit by player");
            GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
            foreach (GameObject cp in checkpoints) {
                cp.GetComponent<Checkpoint>().isLastCheckpoint = false;
            }
            isLastCheckpoint = true;
        }
    }
}
