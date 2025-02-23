using UnityEngine;
using UnityEngine.InputSystem;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private string destination;
    private Vector3 defaultScale;
    private Vector3 selectedScale = new Vector3(3, 3, 1);
    private float scaleSmoothFactor = 10f;
    private bool selected = false;
    private InputAction attackAction;

    void Start() {
        defaultScale = transform.localScale;
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
            SceneTransition.LoadScene(destination);
        }
        transform.localScale = Vector3.Lerp(transform.localScale, selected ? selectedScale : defaultScale, 1 - Mathf.Exp(-scaleSmoothFactor * Time.deltaTime));
    }
}
