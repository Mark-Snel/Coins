using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ConnectionManager {
    private static IConnection connection;
    static ConnectionManager() {
        Application.quitting += OnApplicationQuit;
    }

    public static void Connect(string address) {
        connection = new WebSocketConnection();
        connection.OnDataReceived += (byte[] receivedData) => {
            byte packetType = receivedData[0];

            if (packetType >= 0 && packetType < Deserialize.Length && Deserialize[packetType] != null) {
                Deserialize[packetType].Invoke(receivedData, 1);
            }
        };
        Dispatcher.Create();
        Dispatcher.ClearFixedUpdate();
        Dispatcher.OnFixedUpdate += () => {
            List<byte> packet = new List<byte>();

            PlayerController.GetDataPacker()?.GetPacket(packet);

            if (packet.Count > 0)
            {
                byte[] finalPacket = packet.ToArray();
                connection.Send(finalPacket);
            }
        };
        connection.Connect(address);
    }

    public static void SubscribeToStatus(Action<ConnectionStatus> handler) {
        connection.OnStatusChanged += handler;
    }

    public static void UnsubscribeFromStatus(Action<ConnectionStatus> handler) {
        connection.OnStatusChanged -= handler;
    }

    private static void OnApplicationQuit() {
        connection?.Disconnect();
    }

    private static Action<byte[], int>[] Deserialize = new Action<byte[], int>[] {
        (byte[] data, int index) => {//ping
            byte[] pong = new byte[] {0, 2, 0, 4, 4, 0, 1, 0, 2, 7};
            connection.Send(pong);
        },
        (byte[] data, int index) => {//connect response
            connection.Connected();
        },
        (byte[] data, int index) => {//current map
            byte mapId = data[index];
            Dispatcher.Enqueue(() => GameController.CheckMap(mapId));
            index++;
            continueDeserializing(data, index);
        },
        (byte[] data, int index) => {//playerdata
            int playerDataIndex = index;
            Dispatcher.Enqueue(() => GameController.ExternalPlayer(data, playerDataIndex));
            index += 46;
            continueDeserializing(data, index);
        }
    };

    private static void continueDeserializing(byte[] data, int index) {
        if (index < data.Length) {
            byte packetType = data[index];
            if (packetType >= 0 && packetType < Deserialize.Length && Deserialize[packetType] != null) {
                Deserialize[packetType].Invoke(data, index + 1);
            }
        }
    }
}
