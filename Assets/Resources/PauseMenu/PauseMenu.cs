using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PauseMenu : MonoBehaviour{
    private Camera mainCam;
    private Transform coverTransform;
    private SpriteRenderer coverSpriteRenderer;
    private Button resumeButton;
    private Button exitButton;
    private Vector3 originalExitPosition;
    private Vector3 originalResumePosition;
    private GameObject DCText;
    private TMP_Text coinsText;
    private int addEffectTimer = 0;
    private int loseEffectTimer = 0;
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

    public void LoseCoins() {
        loseEffectTimer = 0;
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
        coinsText = transform.Find("Canvas").transform.Find("Info").Find("Coins").GetComponent<TMP_Text>();
    }

    void Start() {
        AttachToMainCamera();
    }

    void FixedUpdate() {
        if (coinsText) {
            if (active) {
                if (GameController.EarnedCoins > 0) {
                    addEffectTimer++;
                    if ((addEffectTimer - 50) % 5 == 1) {
                        GameController.EarnedCoins--;
                        GameController.Coins++;
                        coinsText.fontSize = 0.4f;
                    }
                } else {
                    addEffectTimer = 0;
                }
                if (GameController.LostCoins > 0) {
                    loseEffectTimer++;
                    if ((loseEffectTimer - 50) % 2 == 1) {
                        GameController.LostCoins--;
                        GameController.Coins--;
                        coinsText.fontSize = 0.4f;
                    }
                } else {
                    loseEffectTimer = 0;
                }
            }
            coinsText.text = $"Coins: {GameController.Coins}" + (GameController.EarnedCoins > 0 ? $" + {GameController.EarnedCoins}" : "") + (GameController.LostCoins > 0 ? $" - {GameController.LostCoins}" : "");
        }
    }

    void Update() {
        if (Camera.main != mainCam) {
            AttachToMainCamera();
        }
        if (active) {
            coinsText.fontSize = Mathf.Lerp(coinsText.fontSize, 0.3f, Time.deltaTime * 5f);
        }

        if (mainCam != null && coverSpriteRenderer != null) {
            float height = mainCam.orthographicSize * 2f;
            float width = height * mainCam.aspect;
            exitButton.transform.position = new Vector3(width / 2 - 0.75f, exitButton.transform.position.y, exitButton.transform.position.z);

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
