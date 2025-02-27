using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketConnection : IConnection {
    public event Action<byte[]> OnDataReceived;
    public event Action<ConnectionStatus> OnStatusChanged;

    private ClientWebSocket webSocket;
    private Uri serverUri;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

    public async void Connect(string address) {
        Status = ConnectionStatus.Connecting;
        OnStatusChanged?.Invoke(Status);

        try {
            serverUri = new Uri("ws://"+address);
            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(serverUri, CancellationToken.None);

            Status = ConnectionStatus.Connected;
            OnStatusChanged?.Invoke(Status);

            Dispatcher.Enqueue(() => GameController.Create());

            _ = ReceiveData(); // Start listening for messages
            SendConnectMessage();
        } catch (Exception ex) {
            Dispatcher.Enqueue(() => {
                Debug.Log("WebSocket Connection Failed: " + ex.Message);
                Status = ConnectionStatus.Disconnected;
                OnStatusChanged?.Invoke(Status);
            });
        }
    }

    public async void Disconnect() {
        if (webSocket != null && webSocket.State == WebSocketState.Open) {
            SendDisconnectMessage();
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

        Status = ConnectionStatus.Disconnected;
        OnStatusChanged?.Invoke(Status);
    }

    public async void Send(byte[] data) {
        if (webSocket != null && webSocket.State == WebSocketState.Open) {
            try {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(data),
                                          WebSocketMessageType.Binary,
                                          true,
                                          CancellationToken.None);
            } catch (WebSocketException ex) {
                Debug.LogWarning("WebSocket send error: " + ex.Message);
                Disconnect();
            }
        }
    }

    public void Connected() {
        Dispatcher.Enqueue(() => {
            GameController.Create();
            Status = ConnectionStatus.Connected;
            OnStatusChanged?.Invoke(Status);
        });
    }

    private async Task ReceiveData() {
        byte[] buffer = new byte[1024];
        while (webSocket != null && webSocket.State == WebSocketState.Open) {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close) {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed connection", CancellationToken.None);
                Status = ConnectionStatus.Disconnected;
                OnStatusChanged?.Invoke(Status);
                return;
            }

            byte[] receivedData = new byte[result.Count];
            Array.Copy(buffer, receivedData, result.Count);
            OnDataReceived?.Invoke(receivedData);
        }
    }

    private void SendConnectMessage() {
        byte[] data = new byte[] { 1, 2, 0, 4, 4, 0, 1, 0, 2, 7, 1, 0, 7, 0 };
        Send(data);
    }

    private void SendDisconnectMessage() {
        if (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting) {
            byte[] data = new byte[] { 2, 2, 0, 4, 4, 0, 1, 0, 2, 7, 3, 0, 7, 0 };
            Send(data);
        }
    }
}
