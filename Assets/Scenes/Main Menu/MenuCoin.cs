using UnityEngine;

public class MenuCoin : MonoBehaviour
{
    public float transitionSpeed = 60f;
    public float rotationIntensity = 515f;
    private float progress = 0f; //Progress value from 0 (start) to 1 (target)
    private float progressY = 0f; //Also that but different timing
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private MenuAction? setAction;
    private MenuButton[] buttons;
    private Transform oins;
    private Vector3 oinsStartPosition;

    void Start() {
        startPosition = transform.position;
        oins = GameObject.Find("Oins")?.transform;
        if (oins == null) {
            Debug.LogWarning("Oins not found");
        } else {
            oinsStartPosition = oins.position;
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
                    foreach (MenuButton button in buttons)
                    {
                        button.active = false;
                    }
                    transform.position = new Vector3(0, transform.localScale.y + Camera.main.orthographicSize, transform.position.z);

                    float distance = Camera.main.orthographicSize - oinsStartPosition.y + (oins.localScale.y/2);
                    float i = Mathf.Min((Mathf.Max(oins.position.y - oinsStartPosition.y, 0.1f) / distance) * 25 * Time.deltaTime, 0.4f);
                    Vector3 position = Vector3.Lerp(oins.position, new Vector3(oinsStartPosition.x, oinsStartPosition.y + distance, oinsStartPosition.z), i);
                    oins.position = position;
                    break;
                default:
                    progressY = 0;
                    setAction = null;
                    break;
            }
        }
    }
}

public enum MenuAction {
    Play,
    Colors
}
