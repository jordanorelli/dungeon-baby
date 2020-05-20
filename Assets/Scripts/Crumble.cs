using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crumble : MonoBehaviour
{
    public Material activeMaterial;
    public Material inactiveMaterial;
    bool isCrumbled;

    private MeshRenderer mesh;

    // Start is called before the first frame update
    void Start() {
        isCrumbled = false;
        mesh = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (isCrumbled) {
            mesh.material = inactiveMaterial;
            GetComponent<BoxCollider2D>().enabled = false;
        } else {
            mesh.material = activeMaterial;
            GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    public void Hit() {
        isCrumbled = true;
        StartCoroutine("Regenerate");
    }

    public IEnumerator Regenerate() {
        yield return new WaitForSeconds(3f);
        isCrumbled = false;
    }
}
