using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveControllerDebugDisplay : MonoBehaviour {
    public GameObject target;
    public Text leftDisplay;
    public Text rightDisplay;
    public Text aboveDisplay;
    public Text belowDisplay;

    private MoveController moveController;

    // Start is called before the first frame update
    void Start() {
        if (target) {
            moveController = target.GetComponent<MoveController>();
        }
    }

    // Update is called once per frame
    void Update() {
        if (moveController) {
            if (moveController.collisions.above) {
                aboveDisplay.text = "True";
            } else {
                aboveDisplay.text = "False";
            }

            if (moveController.collisions.below) {
                belowDisplay.text = "True";
            } else {
                belowDisplay.text = "False";
            }

        }
    }
}
