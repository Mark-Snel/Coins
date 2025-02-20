using UnityEngine;
using UnityEngine.InputSystem;
using static InputHandling;

public class MenuCoin : MonoBehaviour//Very jank, i know, why are you here anyway?
{
    public float transitionSpeed = 60f;
    public float rotationIntensity = 515f;
    private float progress = 0f; //Progress value from 0 (start) to 1 (target)
    private float progressY = 0f; //Also that but different timing
    private float progress1 = 0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private MenuAction? setAction;
    private MenuButton[] buttons;
    private Transform oins;
    private Vector3 oinsStartPosition;
    private Vector3 oinsEndPosition;
    private float oinsDistance;
    private InputAction aimAction;
    public GameObject prefab;
    public GameObject selectorParent;

    void Start() {
        aimAction = InputSystem.actions.FindAction("Aim");
        startPosition = transform.position;
        oins = GameObject.Find("Oins")?.transform;
        if (oins == null) {
            Debug.LogWarning("Oins not found");
        } else {
            oinsStartPosition = oins.position;
            oinsDistance = Camera.main.orthographicSize - oinsStartPosition.y + (oins.localScale.y/2);
            oinsEndPosition = new Vector3(oinsStartPosition.x, oinsStartPosition.y + oinsDistance, oinsStartPosition.z);
        }
    }
    public void Goto(float x, float y, MenuAction action) {
        buttons = FindObjectsByType<MenuButton>(0);

        foreach (MenuButton button in buttons)
        {
            button.locked = true;
        }
        targetPosition = new Vector3(x, y, transform.position.z);
        setAction = action;
    }

    void Update() {
        if (progress < 0.999f && setAction != null)
        {
            //Replicating old js
            progress = (progress + (-(progress*0.5f - 0.5f) * 0.3f)) * progressY;
            progressY = Mathf.Min(progressY + (-0.06f * (Time.deltaTime * transitionSpeed)) * Mathf.Max(progressY - 1.2f, -1), 1);
            progress = Mathf.Clamp01(progress);

            transform.position = new Vector3(
                Mathf.Lerp(startPosition.x, targetPosition.x, progress),
                Mathf.Lerp(startPosition.y, targetPosition.y, progressY),
                transform.position.z
            );

            float newRot = Mathf.Lerp(0, rotationIntensity, progress);
            transform.rotation = Quaternion.Euler(0f, 0f, newRot);
        } else if (progress >= 0.999f) {
            switch (setAction) {
                case MenuAction.Play:
                    break;
                case MenuAction.Colors:
                    selectColor();
                    break;
                default:
                    progressY = 0;
                    setAction = null;
                    break;
            }
        }
    }
    void selectColor() {
        if (oins.position.y < oinsEndPosition.y - 0.01) {
            float i = Mathf.Min((Mathf.Max(oins.position.y - oinsStartPosition.y, 0.1f) / oinsDistance) * 25 * Time.deltaTime, 0.4f);
            Vector3 position = Vector3.Lerp(oins.position, oinsEndPosition, i);
            oins.position = position;
            foreach (MenuButton button in buttons)
            {
                button.active = false;
            }
            transform.position = new Vector3(0, transform.localScale.y + Camera.main.orthographicSize, transform.position.z);
        } else if (transform.position.y != 0){
            Vector3 newPosition = Vector3.Lerp(transform.position, new Vector3(0, 0, transform.position.z), 6 * Time.deltaTime);
            transform.position = newPosition.y < 0.005f ? new Vector3(0, 0, transform.position.z) : newPosition;
            if (transform.position.y == 0) {//Should only work a single frame
                for (int i = 0; i < 24; i++) {
                    int groupIndex = i / 3; //Which group this object belongs to
                    int rowIndex = i % 3;  //Which row within the group

                    float angle = groupIndex * -45; //Base angle for the group
                    float radius = (rowIndex) * 0.5f + 2.4f; //Distance from center

                    Quaternion rotation = Quaternion.Euler(0, 0, angle);
                    GameObject selector = Instantiate(prefab, Vector3.zero, rotation, selectorParent.transform);
                    Transform innerObject = selector.transform.Find("Inner");
                    innerObject.localPosition = new Vector3(radius, 0, 0);
                    Transform deepObject = innerObject.Find("Deep");
                    deepObject.GetComponent<ObjectDecorator>().Color = i;
                    deepObject.localScale = new Vector3(0.2f, 1, 0);
                }
            }
            transform.rotation = Quaternion.Euler(0, 0, GetRotationToCursor(Vector3.zero, aimAction, Camera.main) - 45);
        } else {
            transform.rotation = Quaternion.Euler(0, 0, GetRotationToCursor(Vector3.zero, aimAction, Camera.main) - 45);
            progress1 = Mathf.Lerp(progress1, 1, 1 - Mathf.Exp(-Time.deltaTime));
            selectorParent.transform.Rotate(0, 0, (1 - progress1) * 17 + 8 * Time.deltaTime);
            foreach (Transform selector in selectorParent.transform) {
                Transform innerObject = selector.Find("Inner");
                Transform deepObject = innerObject.Find("Deep");
                deepObject.localScale = new Vector3(Mathf.Max(Mathf.Min(progress1 * 2f, 1), deepObject.localScale.x), deepObject.localScale.y, deepObject.localScale.z);
                selector.GetComponent<ColorSelector>().Expand(Mathf.Min(progress1 * 2f, 1));
            }
        }
    }
}

public enum MenuAction {
    Play,
    Colors
}
