using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
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

    // Used to ignore clicks immediately after the input field is selected.
    private float lastInputSelectTime = 0f;
    private const float inputSelectIgnoreDelay = 0.2f; // seconds

    void Start() {
        coin = transform.Find("Coin");
        coinDecorator = coin.GetComponent<ObjectDecorator>();
        feedback = transform.Find("Feedback").GetComponent<TextDecorator>();
        ipInput = transform.Find("IpInput").GetComponent<TMP_InputField>();

        // Subscribe to onEndEdit and onSelect events.
        //ipInput.onEndEdit.AddListener(OnInputEndEdit);
        // Apparently ^ isnt needed because enter is an attackinput or smth?
        ipInput.onSelect.AddListener(HandleInputSelect);

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

        // Subscribe to the button callbacks if this object also has a Button component.
        var btn = GetComponent<Button>();
        if (btn != null) {
            btn.OnClick = () => {
                // Prevent click if the input field was just selected.
                if (Time.time - lastInputSelectTime < inputSelectIgnoreDelay || ipInput.isFocused)
                    return;
                // Only call Submit if the input field has at least 3 characters.
                if (ipInput.text.Length >= 3)
                    Submit();
            };

            btn.CanClickEffect = () => {
                // Do not run click effect if the input field was just selected.
                if (Time.time - lastInputSelectTime < inputSelectIgnoreDelay || ipInput.isFocused)
                    return false;
                return ipInput.text.Length >= 3;
            };
        }
    }

    void HandleInputSelect(string ignored) {
        lastInputSelectTime = Time.time;
    }

    void OnInputEndEdit(string text) {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            Submit();
        }
    }

    void Submit() {
        Dispatcher.Initialize();
        connectedIp = ipInput.text;
        _ = ConnectionManager.Connect(connectedIp);
    }

    public static void StartConnecting() {
        OnlineButton instance = FindFirstObjectByType<OnlineButton>(0);
        if (instance != null) {
            instance.feedback.ChangeText("Connecting...");
            instance.coinDecorator.Color = -1;
            instance.connecting = true;
        }
    }

    public static void FailConnection() {
        OnlineButton instance = FindFirstObjectByType<OnlineButton>(0);
        if (instance != null) {
            instance.coinDecorator.Color = 1;
            instance.feedback.ChangeText("Connection Failed");
            instance.connecting = false;
        }
    }
    public static void SucceedConnection() {
        OnlineButton instance = FindFirstObjectByType<OnlineButton>(0);
        if (instance != null) {
            instance.coinDecorator.Color = 10;
            instance.feedback.ChangeText("Game Found");
            instance.connecting = false;
            PlayerPrefs.SetString("ip", instance.connectedIp);
            PlayerPrefs.Save();
        }
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
