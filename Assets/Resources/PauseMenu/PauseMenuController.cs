using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController {
    private static PauseMenu menu;
    public static bool enabled = false;
    private static InputAction pauseAction;

    public static void Initialize() {
        pauseAction = InputSystem.actions.FindAction("Pause");
        pauseAction.performed += ctx => Pause();
        pauseAction.Enable();
    }

    public static void GameDisconnected() {
        if (enabled) {
            createMenu();
            if (menu != null) {
                menu.GameDisconnected();
            }
        }
    }

    public static void Exit() {
        SceneTransition.LoadScene("Main Menu");
        Resume();
        menu.Reset();
    }
    public static void Resume() {
        if (menu != null) {
            menu.Active = false;
        }
    }

    public static void Open() {
        createMenu();
        if (!menu.locked) {
            if (enabled) {
                menu.Active = true;
            }
        }
    }

    public static void Pause() {
        createMenu();
        if (!menu.locked) {
            if (menu.Active) {
                menu.Active = false;
            } else if (enabled) {
                menu.Active = true;
            }
        }
    }

    private static void createMenu() {
        if (menu == null) {
            GameObject pausePrefab = Resources.Load<GameObject>("PauseMenu/Pause");
            if (pausePrefab != null) {
                GameObject pauseInstance = Object.Instantiate(pausePrefab);
                menu = pauseInstance.GetComponent<PauseMenu>();
                if (menu == null) {
                    Debug.LogError("The instantiated pause prefab does not have a PauseMenu component.");
                }
            } else {
                Debug.LogError("Pause prefab not found at Resources/PauseMenu/Pause");
            }
        }
    }
}
