using UnityEngine;

public class PauseMenu : MonoBehaviour{
    private Camera mainCam;
    private Transform coverTransform;
    private SpriteRenderer coverSpriteRenderer;
    private Button resumeButton;
    private Button exitButton;
    private Vector3 originalExitPosition;
    private Vector3 originalResumePosition;
    private GameObject DCText;
    public bool locked { get; private set; }
    private bool active = false;
    public bool Active {
        get {
            return active;
        }
        set {
            if (active != value) {
                active = value;
                gameObject.SetActive(value);
                if (active && resumeButton && exitButton) {
                    resumeButton.selected = false;
                    exitButton.selected = false;
                }
            }
        }
    }

    void Awake() {
        DontDestroyOnLoad(gameObject);
        Transform cover = transform.Find("Cover");
        if (cover != null) {
            coverTransform = cover;
            coverSpriteRenderer = cover.GetComponent<SpriteRenderer>();
            if (coverSpriteRenderer == null) {
                Debug.LogError("Cover object does not have a SpriteRenderer!");
            }
        } else {
            Debug.LogError("Child object named 'Cover' not found.");
        }
        Transform resumeTrans = transform.Find("Canvas").Find("Resume");
        if (resumeTrans != null) {
            resumeButton = resumeTrans.GetComponent<Button>();
            if (resumeButton != null) {
                resumeButton.OnClick += PauseMenuController.Resume;
                originalResumePosition = resumeButton.transform.localPosition;
            } else
                Debug.LogWarning("Resume button found but has no Button component.");
        } else {
            Debug.LogWarning("Resume button not found.");
        }

        Transform exitTrans = transform.Find("Canvas").Find("Exit");
        if (exitTrans != null) {
            exitButton = exitTrans.GetComponent<Button>();
            if (exitButton != null) {
                exitButton.OnClick += PauseMenuController.Exit;
                originalExitPosition = exitButton.transform.localPosition;
            } else
                Debug.LogWarning("Exit button found but has no Button component.");
        } else {
            Debug.LogWarning("Exit button not found.");
        }
        DCText = transform.Find("Canvas").Find("DC").gameObject;
    }

    void Start() {
        AttachToMainCamera();
    }

    void Update() {
        if (Camera.main != mainCam) {
            AttachToMainCamera();
        }

        if (mainCam != null && coverSpriteRenderer != null) {
            float height = mainCam.orthographicSize * 2f;
            float width = height * mainCam.aspect;

            Vector2 spriteSize = coverSpriteRenderer.sprite.bounds.size;

            Vector3 newScale = new Vector3(width, height, 1f);
            coverTransform.localScale = newScale;
        }
    }

    void AttachToMainCamera() {
        mainCam = Camera.main;
        if (mainCam != null) {
            transform.SetParent(mainCam.transform, false);
            transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
        } else {
            Debug.LogWarning("No main camera found.");
        }
    }

    public void GameDisconnected() {
        Active = true;
        locked = true;
        exitButton.transform.localPosition = new Vector3(0, originalExitPosition.y, originalExitPosition.z);
        resumeButton.transform.localPosition = new Vector3(originalResumePosition.x, -4, originalResumePosition.z);
        DCText.SetActive(true);
    }

    public void Reset() {
        locked = false;
        Active = false;
        exitButton.transform.localPosition = originalExitPosition;
        resumeButton.transform.localPosition = originalResumePosition;
        DCText.SetActive(false);
    }
}
