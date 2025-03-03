using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*Client sent packets:
 0 *= pong
 1 = connect
 2 = disconnect
 3 = playerdata
 */
/*Server sent packets:
 0 *= ping
 1 = connect response
 2 = current map
 3 = playerdata
 4 = playerId
 5 = playerIdList
 */

public class ConnectionManager {
    private static IConnection connection;

    public static void Disconnected() {
        PauseMenuController.GameDisconnected();
        OnlineButton.FailConnection();
    }
    public static void Connected() {
        OnlineButton.SucceedConnection();
    }
    public static void Connecting() {
    }

    static ConnectionManager() {
        Application.quitting += OnApplicationQuit;
    }

    public static void Disconnect() {
        if (connection != null) {
            connection.Disconnect();
        }
    }

    public static void Connect(string address) {
        if (connection != null) {
            connection.Disconnect();
        }
        #if UNITY_WEBGL
            connection = new WebSocketConnection();
        #else
            connection = new UdpConnection();
        #endif
        Debug.Log("Using connection type: " + connection.GetType().Name);
        connection.OnDataReceived += (byte[] receivedData) => {
            byte packetType = receivedData[0];

            if (packetType >= 0 && packetType < Deserialize.Length && Deserialize[packetType] != null) {
                Deserialize[packetType].Invoke(receivedData, 1);
            }
        };
        Dispatcher.Initialize();
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
        try {
            connection.Connect(address);
        } catch {
            connection.Disconnect();
        }
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
            Dispatcher.MainNow(_ => GameController.ExternalPlayer(data, playerDataIndex));
            index += PlayerPacker.PacketLength;
            continueDeserializing(data, index);
        },
        (byte[] data, int index) => {//playerId
            GameController.playerId = data[index];
            index++;
            continueDeserializing(data, index);
        },
        (byte[] data, int index) => {//playerIdsList
            byte length = data[index++];
            int playerListIndex = index;
            GameController.CheckPlayers(data, playerListIndex, length);
            index += length;
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
