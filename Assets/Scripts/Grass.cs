using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour {
    public bool isEaten;
    public Mesh uneatenMesh;
    public Mesh eatenMesh;

    private MeshFilter meshFilter;

    // Start is called before the first frame update
    void Start() {
        meshFilter = GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    void Update() {
        if (isEaten) {
            meshFilter.mesh = eatenMesh;
        } else {
            meshFilter.mesh = uneatenMesh;
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player") || collider.CompareTag("Enemy")) {
            isEaten = true;
        }
    }
}
