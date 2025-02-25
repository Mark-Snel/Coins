using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OnlineButton : MonoBehaviour
{
    private TMP_InputField ipInput;
    private TextDecorator feedback;
    private Transform coin;
    private ObjectDecorator coinDecorator;
    private bool connecting = false;
    private string connectedIp;
    public float rotationSpeed = -360;

    void Start() {
        coin = transform.Find("Coin");
        coinDecorator = coin.GetComponent<ObjectDecorator>();
        feedback = transform.Find("Feedback").GetComponent<TextDecorator>();
        ipInput = transform.Find("IpInput").GetComponent<TMP_InputField>();
        ipInput.onEndEdit.AddListener(OnInputEndEdit);

        string previousIp = PlayerPrefs.GetString("ip", "");
        ipInput.text = previousIp;
    }

    void OnInputEndEdit(string text) {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            SubmitInput();
        }
    }

    void SubmitInput() {
        connectedIp = ipInput.text;
        feedback.ChangeText("Connecting...");
        coinDecorator.Color = -1;
        connecting = true;
    }

    void FailConnection() {
        coinDecorator.Color = 1;
        feedback.ChangeText("Connection Failed");
        connecting = false;
    }
    void SucceedConnection() {
        coinDecorator.Color = 10;
        feedback.ChangeText("Game Found");
        connecting = false;
        PlayerPrefs.SetString("ip", connectedIp);
        PlayerPrefs.Save();
    }

    void Update() {
        if (connecting) {
            coin.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}
