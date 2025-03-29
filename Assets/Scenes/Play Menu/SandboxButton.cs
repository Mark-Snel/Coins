using UnityEngine;

public class SandboxButton : MonoBehaviour {
    private Button button;

    void Start() {
        button = GetComponent<Button>();
        if (button != null) {
            button.OnClick -= SetMaxCoins;
            button.OnClick += SetMaxCoins;
        }
    }

    void SetMaxCoins() {
        GameController.Coins = int.MaxValue;
    }
}
