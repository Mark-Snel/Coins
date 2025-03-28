using System;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class WebSocketConnection : IConnection {
    public event Action<byte[]> OnDataReceived;
    public event Action<ConnectionStatus> OnStatusChanged;
    #if !UNITY_WEBGL || UNITY_EDITOR
        private bool isTickSubscribed = false;
    #endif

    private WebSocket webSocket;
    private ConnectionStatus status = ConnectionStatus.Disconnected;
    public ConnectionStatus Status {
        get => status;
        private set {
            if (status != value) {
                status = value;
                OnStatusChanged?.Invoke(status);
            }
        }
    }

    public async Task Connect(string address) {
        #if !UNITY_WEBGL || UNITY_EDITOR
            if (!isTickSubscribed) {
                Dispatcher.OnUpdate += Tick;
                isTickSubscribed = true;
            }
        #endif

        if (Status == ConnectionStatus.Disconnected) {
            Status = ConnectionStatus.Connecting;

            string url = "ws://" + address;
            webSocket = new WebSocket(url);

            // Set up event handlers
            webSocket.OnOpen += () => {
                GameController.Initialize();
                Dispatcher.Enqueue(() => {Status = ConnectionStatus.Connected;});
            };

            webSocket.OnError += (errorMsg) => {
                Debug.Log("WebSocket Error: " + errorMsg);
            };

            webSocket.OnClose += (closeCode) => {
                Dispatcher.Enqueue(() => {Status = ConnectionStatus.Disconnected;});
            };

            webSocket.OnMessage += (bytes) => {
                OnDataReceived?.Invoke(bytes);
            };

            try {
                await webSocket.Connect();
            } catch (Exception ex) {
                Debug.Log("WebSocket Connection Failed: " + ex.Message);
                Dispatcher.Enqueue(() => {Status = ConnectionStatus.Disconnected;});
            }
        }
    }

    public async Task Disconnect() {
        if (webSocket != null) {
            if (webSocket.State == WebSocketState.Open) {
                await webSocket.Close();
            } else if (webSocket.State == WebSocketState.Connecting) {
                Debug.Log("Closed on connecting");
                webSocket = null;
                Status = ConnectionStatus.Disconnected;
            } else if (webSocket.State == WebSocketState.Closed) {
                Status = ConnectionStatus.Disconnected;
            } else {
                Debug.LogError("Shit, i dunno, websocket didnt wanna close or sommet");
            }
        } else {
            Status = ConnectionStatus.Disconnected;
        }
    }

    public async Task Send(byte[] data) {
        if (webSocket != null && webSocket.State == WebSocketState.Open) {
            try {
                await webSocket.Send(data);
            } catch (Exception ex) {
                Debug.LogWarning("WebSocket send error: " + ex.Message);
                await Disconnect();
            }
        }
    }

    public void Tick() {
        #if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
        #endif
    }
}
