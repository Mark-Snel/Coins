using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour {
    private static SceneTransition Instance;
    private static Transform shiny;
    private static Transform screen;
    private static string destination = "";
    private static bool transitioning = false;
    private static bool fadeTransition = false;
    private static float fadeSpeed = 1f;
    private static float curtainSpeed = 2.5f;
    private static SpriteRenderer screenSr;
    private static float progress = 0;

    public static void LoadScene(string sceneName, bool fadeToBlack = false) {
        if (transitioning) return;
        if (Instance == null) {
            GameObject obj = Resources.Load<GameObject>("Transition/SceneTransition");
            Instance = Instantiate(obj.GetComponent<SceneTransition>());
            DontDestroyOnLoad(Instance.gameObject);

            // Get references to children objects.
            screen = Instance.transform.Find("Screen");
            screenSr = screen.GetComponent<SpriteRenderer>();
            shiny = Instance.transform.Find("Shiny");

            // Check if the main camera exists; if not, create one.
            if (Camera.main == null) {
                GameObject newCamera = new GameObject("Main Camera");
                newCamera.AddComponent<Camera>();
                newCamera.tag = "MainCamera";
            }
            // Attach the transition object to the camera.
            Instance.transform.SetParent(Camera.main.transform, false);
        }
        transitioning = true;
        fadeTransition = fadeToBlack;
        destination = sceneName;
        if (!fadeToBlack) {
            UpdateRenderers(Instance.transform, true);
        } else {
            screenSr.enabled = true;
        }
    }

    void LateUpdate() {
        if (transitioning) {
            // Ensure we have a valid camera (in case the camera got unloaded).
            if (Camera.main == null) {
                GameObject newCamera = new GameObject("Main Camera");
                newCamera.AddComponent<Camera>();
                newCamera.tag = "MainCamera";
                // Reattach this transition object.
                transform.SetParent(newCamera.transform, false);
            }
            // Calculate dimensions based on the main camera.
            float cameraHeight = Camera.main.orthographicSize * 2f;
            float cameraWidth = cameraHeight * Camera.main.aspect;
            screen.localScale = new Vector3(cameraWidth, cameraHeight, 1);

            if (!destination.Equals("")) {
                // Update the shiny object using localPosition.
                shiny.localPosition = new Vector3(cameraWidth / 2 + 5.5f, shiny.localPosition.y, shiny.localPosition.z);
                if (fadeTransition) {
                    progress = Mathf.Min(progress + Time.deltaTime * fadeSpeed, 1);
                    screenSr.color = new Color(0, 0, 0, progress);
                    // Reset localPosition to keep it centered.
                    transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
                } else {
                    progress = Mathf.Min(progress + Time.deltaTime * curtainSpeed, 1);
                    // Lerp using local positions relative to the camera.
                    transform.localPosition = Vector3.Lerp(
                        new Vector3(-cameraWidth - 11f, 0, transform.localPosition.z),
                        new Vector3(0, 0, transform.localPosition.z),
                        progress
                    );
                }
                if (progress >= 1) {
                    SceneManager.LoadScene(destination);
                    GameController.Initialize();
                    GameController.Instance?.UpdateMap(destination);
                    destination = "";
                    FlipShiny();
                    UpdateRenderers(transform, true);
                }
            } else {
                progress = Mathf.Max(progress - Time.deltaTime * curtainSpeed, 0);
                transform.localPosition = Vector3.Lerp(
                    new Vector3(cameraWidth + 11f, 0, transform.localPosition.z),
                    new Vector3(0, 0, transform.localPosition.z),
                    progress
                );
                if (progress <= 0) {
                    transitioning = false;
                    FlipShiny();
                    UpdateRenderers(transform, false);
                    Destroy(gameObject);
                }
            }
        }
    }

    void FlipShiny() {
        shiny.localScale = new Vector3(shiny.localScale.x * -1, shiny.localScale.y * -1, shiny.localScale.z);
        shiny.localPosition = new Vector3(shiny.localPosition.x * -1, shiny.localPosition.y, shiny.localPosition.z);
    }

    static void UpdateRenderers(Transform obj, bool enabled) {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (sr != null) {
            sr.enabled = enabled;
        }
        if (mr != null) {
            mr.enabled = enabled;
        }
        foreach (Transform child in obj) {
            UpdateRenderers(child, enabled);
        }
    }
}