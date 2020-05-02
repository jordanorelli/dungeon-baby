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
    public Text jumpStateDisplay;

    private PlayerController player;
    private MoveController moveController;

    // Start is called before the first frame update
    void Start() {
        if (target) {
            moveController = target.GetComponent<MoveController>();
            player = target.GetComponent<PlayerController>();
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
        if (player) {
            switch (player.jumpState) {
            case PlayerController.JumpState.Grounded:
                jumpStateDisplay.text = "Grounded";
                break;
            case PlayerController.JumpState.CoyoteTime:
                jumpStateDisplay.text = "CoyoteTime";
                break;
            case PlayerController.JumpState.Ascending:
                jumpStateDisplay.text = "Ascending";
                break;
            case PlayerController.JumpState.Apex:
                jumpStateDisplay.text = "Apex";
                break;
            case PlayerController.JumpState.Descending:
                jumpStateDisplay.text = "Descending";
                break;
            case PlayerController.JumpState.Falling:
                jumpStateDisplay.text = "Falling";
                break;
            default:
                jumpStateDisplay.text = "???";
                break;
            }
        
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
