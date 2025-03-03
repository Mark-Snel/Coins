using System;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class WebSocketConnection : IConnection {
    public event Action<byte[]> OnDataReceived;

    private WebSocket webSocket;
    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

    public async void Connect(string address) {
        Dispatcher.OnUpdate += Tick;
        Status = ConnectionStatus.Connecting;
        Dispatcher.Enqueue(ConnectionManager.Connecting);

        // Make sure the URL includes the proper protocol (ws:// or wss://)
        string url = "ws://" + address;
        webSocket = new WebSocket(url);

        // Set up event handlers
        webSocket.OnOpen += () => {
            Dispatcher.Enqueue(SendConnectMessage);
        };

        webSocket.OnError += (errorMsg) => {
            Debug.Log("WebSocket Error: " + errorMsg);
        };

        webSocket.OnClose += (closeCode) => {
            Status = ConnectionStatus.Disconnected;
            Dispatcher.Enqueue(ConnectionManager.Disconnected);
        };

        webSocket.OnMessage += (bytes) => {
            OnDataReceived?.Invoke(bytes);
        };

        try {
            await webSocket.Connect();
        } catch (Exception ex) {
            Debug.Log("WebSocket Connection Failed: " + ex.Message);
            Status = ConnectionStatus.Disconnected;
            Dispatcher.Enqueue(ConnectionManager.Disconnected);
        }
    }

    public void Connected() {
        Dispatcher.Enqueue(() => {
            GameController.Initialize();
            Status = ConnectionStatus.Connected;
            Dispatcher.Enqueue(ConnectionManager.Connected);
        });
    }

    public async void Disconnect() {
        if (webSocket != null) {
            SendDisconnectMessage();
            await webSocket.Close();
        }
    }

    public async void Send(byte[] data) {
        if (webSocket != null && webSocket.State == WebSocketState.Open) {
            try {
                await webSocket.Send(data);
            } catch (Exception ex) {
                Debug.LogWarning("WebSocket send error: " + ex.Message);
                Disconnect();
            }
        }
    }

    // Sends an initial connect message (customize as needed).
    private void SendConnectMessage() {
        byte[] data = new byte[] { 1, 2, 0, 4, 4, 0, 1, 0, 2, 7, 1, 0, 7, 0 };
        Send(data);
    }

    // Sends a disconnect message (customize as needed).
    private void SendDisconnectMessage() {
        if (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting) {
            byte[] data = new byte[] { 2, 2, 0, 4, 4, 0, 1, 0, 2, 7, 3, 0, 7, 0 };
            Send(data);
        }
    }

    // IMPORTANT: On WebGL, NativeWebSocket requires you to call DispatchMessageQueue regularly.
    // Expose this method so that a MonoBehaviour can call it from its Update() method.
    public void Tick() {
        #if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
        #endif
    }
}
