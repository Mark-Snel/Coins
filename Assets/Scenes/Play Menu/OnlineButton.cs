using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

        string hostAndPort = "";

        #if UNITY_WEBGL && !UNITY_EDITOR
        string fullUrl = Application.absoluteURL;
        hostAndPort = GetHostAndPort(fullUrl);
        #endif

        if (string.IsNullOrEmpty(hostAndPort))
        {
            hostAndPort = PlayerPrefs.GetString("ip", "");
        }

        ipInput.text = hostAndPort;
    }

    void OnInputEndEdit(string text) {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            Submit();
        }
    }

    public void Submit() {
        connectedIp = ipInput.text;
        feedback.ChangeText("Connecting...");
        coinDecorator.Color = -1;
        connecting = true;
        ConnectionManager.Connect(connectedIp);
        ConnectionManager.SubscribeToStatus(HandleStatusChanged);
    }

    private void HandleStatusChanged(ConnectionStatus status) {
        if (status == ConnectionStatus.Disconnected) {
            FailConnection();
        } else if (status == ConnectionStatus.Connected) {
            SucceedConnection();
        }

        ConnectionManager.UnsubscribeFromStatus(HandleStatusChanged);
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

    string GetHostAndPort(string url) {
        try {
            Uri uri = new Uri(url);
            return uri.Authority; // Returns "123.123.123.123:1234" or "example.com:8080"
        } catch (Exception e) {
            Debug.LogError("Invalid URL: " + e.Message);
            return "";
        }
    }
}
