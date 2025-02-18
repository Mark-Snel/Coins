using UnityEngine;
using UnityEngine.InputSystem;
public class MenuButton : MonoBehaviour
{
    public Vector3 selectedScale = new Vector3(1.7f, 1.7f, 1);
    public MenuAction action = MenuAction.Play;
    public bool locked = false;
    public bool active = true;
    private Transform text;
    private bool selected = false;
    private Vector3 defaultScale;
    private Vector3 defaultPosition;
    private Vector3 defaultTextPosition;
    private float scaleSmoothFactor = 0.25f;
    private float positionSmoothFactor = 25f;
    private InputAction attackAction;
    private MenuCoin coin;

    void Start() {
        defaultScale = transform.localScale;
        defaultPosition = transform.position;

        string baseName = gameObject.name.Replace("Button", "");
        text = GameObject.Find(baseName)?.transform;
        defaultTextPosition = text.position;

        if (text == null) Debug.LogWarning("No text found for " + baseName);

        attackAction = InputSystem.actions.FindAction("Attack");
        coin = FindFirstObjectByType<MenuCoin>();
    }
    void OnMouseEnter() {
        if (!locked && active) {
            selected = true;
        }
    }

    void OnMouseExit() {
        if (!locked) {
            selected = false;
        }
    }

    void Update() {
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        if (attackAction.WasCompletedThisFrame() && selected && !locked && active) {
            coin.Goto(transform.position.x + transform.localScale.x/2, transform.position.y, action);
            locked = true;
        }
        Vector3 scale = Vector3.Lerp(transform.localScale, selected ? selectedScale : defaultScale, scaleSmoothFactor);

        float i = Mathf.Min((Mathf.Max(defaultPosition.x - transform.position.x, 0.1f) / cameraWidth) * positionSmoothFactor * Time.deltaTime, 0.4f);
        Vector3 position = Vector3.Lerp(transform.position, active ?
        defaultPosition
        : new Vector3(defaultPosition.x - cameraWidth / 2, defaultPosition.y, defaultPosition.z), i);
        Vector3 textPosition = Vector3.Lerp(text.position, active ?
        defaultTextPosition
        : new Vector3(defaultTextPosition.x - cameraWidth / 2, defaultTextPosition.y, defaultTextPosition.z), i);

        transform.localScale = scale;
        text.localScale = scale;
        transform.position = position;
        text.position = textPosition;
    }
}
