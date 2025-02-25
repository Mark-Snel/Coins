using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;

public class UdpConnection : MonoBehaviour
{
    private float lastMessageTime;
    private const float timeoutDuration = 14f;
    private bool isClosing = false;
    private static UdpConnection instance;
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    private string ip;
    private int port;
    private static ConnectionStatus status = ConnectionStatus.Disconnected;
    public static ConnectionStatus Status {
        get {return status;}
    }

    public static void Connect(string address) {
        status = ConnectionStatus.Connecting;
        Dispatcher.Instantiate();
        if (instance == null)
        {
            GameObject obj = new GameObject("UdpConnection");
            instance = obj.AddComponent<UdpConnection>();
            DontDestroyOnLoad(instance);
        }
        try {
            string[] addressParts = address.Split(':');
            if (addressParts.Length < 2) {
                status = ConnectionStatus.Disconnected;
                return;
            }
            instance.ip = addressParts[0];
            instance.port = int.Parse(addressParts[1]);

            instance.serverEndPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            instance.udpClient = new UdpClient();
            instance.ReceiveData();
            instance.lastMessageTime = Time.time;
            instance.StartCoroutine(instance.CheckForTimeout());
            instance.SendMessageToServer("hello server, it is i, coins client");
        } catch (Exception ex) {
            status = ConnectionStatus.Disconnected;
            Debug.LogError("Error connecting to server: " + ex.Message);
        }
    }


    public void SendMessageToServer(string message) {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, serverEndPoint);
    }

    private IEnumerator CheckForTimeout() {
        while (status == ConnectionStatus.Connected || status == ConnectionStatus.Connecting) {
            yield return new WaitForSeconds(1f);
            if (Time.time - lastMessageTime > timeoutDuration) {
                Debug.Log("Connection timed out, disconnecting.");
                Disconnect();
            }
            if (status == ConnectionStatus.Connecting) {
                SendMessageToServer("hello server, it is i, coins client");
            }
        }
    }

    void Disconnect() {
        isClosing = true;
        SendMessageToServer("disconnecting");
        status = ConnectionStatus.Disconnected;
        udpClient.Close();
    }

    private void ReceiveData() {
        if (isClosing) return;
        udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    private void ReceiveCallback(IAsyncResult ar) {
        if (isClosing) return;
        try {
            byte[] receivedData = udpClient.EndReceive(ar, ref serverEndPoint);
            string receivedMessage = Encoding.UTF8.GetString(receivedData);

            Dispatcher.Enqueue(() => ProcessReceivedMessage(receivedMessage));

            ReceiveData();
        } catch (Exception ex) {
            Debug.LogError("Error receiving data: " + ex.Message);
        }
    }

    private void ProcessReceivedMessage(string receivedMessage) {
        instance.lastMessageTime = Time.time;
        if (receivedMessage.Equals("ping")) {
            SendMessageToServer("pong");
        }
        if (receivedMessage.Equals("greetings coins client, a wonderous meeting indeed")) {
            status = ConnectionStatus.Connected;
        }
    }

    void OnDestroy() {
        try {
            Disconnect();
        } catch (Exception ex) {
            Debug.LogError("Error during cleanup: " + ex.Message);
        }
    }
}

public enum ConnectionStatus {
    Disconnected,
    Connecting,
    Connected
}
