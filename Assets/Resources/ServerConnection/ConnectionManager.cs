using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

/*Client sent packets:
 0 *= pong
 1 =
 2 = roundOverConfirm
 3 = playerdata
 4 = shots
 5 = hits
 */
/*Server sent packets:
 0 *= ping
 1 =
 2 = current map
 3 = playerdata
 4 = playerId
 5 = playerIdList
 6 = shots
 7 = hits
 8 = roundOverData
 9 = newRound
 */

public class ConnectionManager {
    private static IConnection connection;
    public static bool sendUpdates {get; private set;} = true;

    static ConnectionManager() {
        Application.quitting += OnApplicationQuit;
    }

    public static void Disconnect() {
        if (connection != null) {
            connection.Disconnect();
        }
    }

    private static Action<byte[]> onDataReceivedHandler = (byte[] receivedData) => {
        byte packetType = receivedData[0];

        if (packetType >= 0 && packetType < Deserialize.Length && Deserialize[packetType] != null) {
            Deserialize[packetType].Invoke(receivedData, 1);
        }
    };

    private static Action<ConnectionStatus> OnStatusChangedHandler = (ConnectionStatus status) => {
        Debug.Log("Connection OnStatusChanged " + status);
        switch (status) {
            case ConnectionStatus.Connected:
                OnlineButton.SucceedConnection();
                break;
            case ConnectionStatus.Connecting:
                OnlineButton.StartConnecting();
                break;
            case ConnectionStatus.Disconnected:
                OnlineButton.FailConnection();
                PauseMenuController.GameDisconnected();
                break;
        }
    };

    public static async Task Connect(string address) {
        Dispatcher.Initialize();
        if (connection != null) {
            await connection.Disconnect();
        } else {
            connection = new WebSocketConnection();
        }
        connection.OnDataReceived -= onDataReceivedHandler;
        connection.OnDataReceived += onDataReceivedHandler;

        connection.OnStatusChanged -= OnStatusChangedHandler;
        connection.OnStatusChanged += OnStatusChangedHandler;

        Dispatcher.ClearFixedUpdate();
        Dispatcher.OnFixedUpdate += () => {
            if (ConnectionManager.sendUpdates) {
                List<byte> packet = new List<byte>();

                PlayerController.GetDataPacker()?.GetPacket(packet);
                WeaponPacker.GetShotsPacket(packet);
                WeaponPacker.GetHitsPacket(packet);

                if (packet.Count > 0) {
                    byte[] finalPacket = packet.ToArray();
                    connection.Send(finalPacket);
                }
            }
        };
        try {
            await connection.Connect(address);
        } catch {
            await connection.Disconnect();
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
            Debug.LogWarning("Huh? connection response? That shouldn't happen!");
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
        },
        (byte[] data, int index) => { //shots
            byte count = data[index++];
            for (byte i = 0; i < count; i++) {
                byte playerId = data[index++];
                int length = BitConverter.ToInt32(data, index);
                index += 4;
                int shotsIndex = index;
                GameController.ShotsFired(data, shotsIndex, length, playerId);
                index += length * WeaponPacker.PerShotPacketSize;
            }
            continueDeserializing(data, index);
        },
        (byte[] data, int index) => { //hits
            byte count = data[index++];
            for (byte i = 0; i < count; i++) {
                int length = BitConverter.ToInt32(data, index);
                index += 4;
                int hitsIndex = index;
                GameController.RegisterHits(data, hitsIndex, length);
                index += length * WeaponPacker.PerHitPacketSize;
            }
            continueDeserializing(data, index);
        },
        (byte[] data, int index) => { //roundOverData
            byte coins = data[index++];
            byte winnerId = data[index++];
            GameController.RoundOver(coins, winnerId);
            ConnectionManager.sendUpdates = false;
            _ = connection.Send(new byte[] {2});
            continueDeserializing(data, index);
        },
        (byte[] data, int index) => { //newRound
            byte spawnOffset = data[index++];
            ConnectionManager.sendUpdates = true;
            GameController.NewRound(spawnOffset);
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
