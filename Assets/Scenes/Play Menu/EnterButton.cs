using UnityEngine;
using UnityEngine.InputSystem;

public class EnterButton : MonoBehaviour
{
    private OnlineButton onlineButton;
    private bool selected = false;
    private InputAction attackAction;

    void Start() {
        onlineButton = transform.parent.GetComponent<OnlineButton>();
        attackAction = InputSystem.actions.FindAction("Attack");
    }

    void OnMouseEnter() {
        selected = true;
    }

    void OnMouseExit() {
        selected = false;
    }

    void Update() {
        if (selected && attackAction.WasCompletedThisFrame()) {
            onlineButton.Submit();
        }
    }
}
