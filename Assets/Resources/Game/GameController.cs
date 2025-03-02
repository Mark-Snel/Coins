using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class GameController : MonoBehaviour
{
    public static byte? playerId  = null;
    public static GameController Instance { get; private set; }

    private static Dictionary<byte, ExternalPlayerController> externalPlayers = new Dictionary<byte, ExternalPlayerController>();

    private static CoinsMap nextMap = CoinsMap.None;
    private static CoinsMap currentMap = CoinsMap.None;
    private GameObject player;
    private GameObject[] respawnPoints;

    public static void Create() {
        if (Instance == null) {
            GameObject obj = Resources.Load<GameObject>("Game/GameController");
            Instance = Instantiate(obj.GetComponent<GameController>());
            DontDestroyOnLoad(Instance);
        }
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Respawn() {

    }

    public static void CheckMap(byte mapIndex) {
        CoinsMap map = System.Enum.IsDefined(typeof(CoinsMap), (int)mapIndex) ? (CoinsMap)(int)mapIndex : CoinsMap.None;
        if (nextMap != map) {
            nextMap = map;
            SceneTransition.LoadScene(map.ToString());
        }
    }

    public void UpdateMap(string mapName) {
        currentMap = GetMapFromString(mapName);
        nextMap = currentMap;
        player = GameObject.FindGameObjectWithTag("Player");
        respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
    }

    public CoinsMap GetCurrentMap() {
        return currentMap;
    }

    CoinsMap GetMapFromString(string sceneName) {
        if (System.Enum.TryParse(sceneName, true, out CoinsMap map)) {
            return map;
        } else {
            return CoinsMap.None;
        }
    }

    public static void ExternalPlayer(byte[] data, int index) {
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data);

        if (playerId == null || currentMap == CoinsMap.None) return;
        //parsing received data
        byte id = span[index + 53];
        if (id == playerId.Value) return;
        bool isDead = span[index] != 0; index += 1;
        int color = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int health = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        float massPerSize = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        float massMultiplier = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        float baseSize = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        int maxHealth = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        float maxHealth_SizeMultiplier = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        float posX = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        float posY = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        Vector2 position = new Vector2(posX, posY);
        float velX = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        float velY = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        Vector2 velocity = new Vector2(velX, velY);
        float forceX = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        float forceY = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        Vector2 force = new Vector2(forceX, forceY);


        ExternalPlayerController player;
        if (!externalPlayers.TryGetValue(id, out player)) {
            GameObject prefab = Resources.Load<GameObject>("Player/ExternalPlayer");
            if(prefab == null)
            {
                Debug.LogError("ExternalPlayer prefab not found in Resources/Player/");
                return;
            }
            GameObject newPlayer = GameObject.Instantiate(prefab, position, Quaternion.identity);
            player = newPlayer.GetComponent<ExternalPlayerController>();
            player.playerId = id;
            externalPlayers.Add(id, player);
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            player.SetPosition(position);
        }

        // Update the player's properties using available public setters.
        player.IsDead = isDead;
        player.Health = health;
        player.MassPerSize = massPerSize;
        player.MassMultiplier = massMultiplier;
        player.BaseSize = baseSize;
        player.MaxHealth = maxHealth;
        player.MaxHealth_SizeMultiplier = maxHealth_SizeMultiplier;
        player.Color = color;

        // Update position and velocity.
        player.UpdatePosition(position);
        player.UpdateVelocity(velocity);
        player.UpdateForce(force);
    }

    public static void CheckPlayers(byte[] data, int index, byte count) {

        bool[] receivedIds = new bool[256];

        for (int i = 0; i < count; i++) {
            if (playerId != null) if (data[index + i] == playerId.Value) continue;
            receivedIds[data[index + i]] = true;
        }

        List<byte> keysToRemove = null;
        foreach (var kvp in externalPlayers) {
            if (!receivedIds[kvp.Key]) {
                if (keysToRemove == null)
                    keysToRemove = new List<byte>();
                keysToRemove.Add(kvp.Key);
            }
        }
        if (keysToRemove != null) {
            foreach (byte key in keysToRemove) {
                if (externalPlayers.TryGetValue(key, out ExternalPlayerController player)) {
                    Dispatcher.Enqueue(() => player.Delete());
                }
                Debug.Log("Removed player " + key);
                externalPlayers.Remove(key);
            }
        }
    }
}

public enum CoinsMap {
    Lobby,
    None
}
