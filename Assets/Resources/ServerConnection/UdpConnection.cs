using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UdpConnection : IConnection {
    public event Action<byte[]> OnDataReceived;
    public event Action<ConnectionStatus> OnStatusChanged;

    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    private float lastMessageTime;
    private const float timeoutDuration = 10f;
    private bool isClosing = false;
    private Coroutine timeoutCoroutine;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;

    public void Connect(string address) {
        Status = ConnectionStatus.Connecting;
        OnStatusChanged?.Invoke(Status);
        isClosing = false;

        string[] addressParts = address.Split(':');
        if (addressParts.Length < 2) {
            Status = ConnectionStatus.Disconnected;
            OnStatusChanged?.Invoke(Status);
            return;
        }
        string ip = addressParts[0];
        int port = int.Parse(addressParts[1]);

        serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        udpClient = new UdpClient();

        ReceiveData();

        lastMessageTime = Time.time;
        Dispatcher.StartCoro(() => CheckForTimeout());

        SendConnectMessage();
    }

    public void Disconnect() {
        isClosing = true;
        SendDisconnectMessage();
        Status = ConnectionStatus.Disconnected;
        Dispatcher.Enqueue(() => {OnStatusChanged?.Invoke(Status);});
        udpClient?.Close();
    }

    public void Send(byte[] data) {
        if (udpClient != null && serverEndPoint != null) {
            udpClient.Send(data, data.Length, serverEndPoint);
        }
    }

    public void Connected() {
        Dispatcher.Enqueue(() => {
            GameController.Initialize();
            Status = ConnectionStatus.Connected;
            OnStatusChanged?.Invoke(Status);
        });
    }

    private void ReceiveData() {
        if (isClosing) return;
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    private void ReceiveCallback(IAsyncResult ar) {
        if (isClosing) return;
        try {
            byte[] receivedData = udpClient.EndReceive(ar, ref serverEndPoint);
            Dispatcher.Enqueue(() => updateTimeout());

            OnDataReceived?.Invoke(receivedData);

            ReceiveData();
        } catch (Exception ex) {
            Debug.LogError("Error receiving data: " + ex.Message);
            ReceiveData();
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

    private IEnumerator CheckForTimeout() {
        while (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting) {
            yield return new WaitForSeconds(1f);
            if (Time.time - lastMessageTime > timeoutDuration) {
                Debug.Log("Connection timed out, disconnecting.");
                Disconnect();
            }
            if (Status == ConnectionStatus.Connecting) {
                SendConnectMessage();
            }
        }
    }

    private void updateTimeout() {
        lastMessageTime = Time.time;
    }
}
