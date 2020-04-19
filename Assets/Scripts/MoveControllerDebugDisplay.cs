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
            showBool(aboveDisplay, moveController.collisions.above);
            showBool(belowDisplay, moveController.collisions.below);
            showBool(leftDisplay, moveController.collisions.left);
            showBool(rightDisplay, moveController.collisions.right);
        }
    }

    void showBool(Text label, bool value) {
        if (value) {
            label.text = "True";
        } else {
            label.text = "False";
        }
    }
}
