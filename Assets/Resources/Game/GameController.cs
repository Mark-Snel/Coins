using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

public class GameController : MonoBehaviour {
    public static int Coins = 0;
    public static int EarnedCoins = 0;
    public static int LostCoins = 0;
    public static int GetTotalCoins() {
        return Coins + EarnedCoins - LostCoins;
    }
    public static void LoseCoins(int amount) {
        LostCoins += amount;
        PauseMenuController.LoseCoins();
    }
    public static byte[] ReceivedPlayerList; // for pause menu info

    public GameObject externalPlayerPrefab;
    public static byte? playerId = null;
    public static GameController Instance { get; private set; }

    private static Dictionary<byte, ExternalPlayerController> externalPlayers = new Dictionary<byte, ExternalPlayerController>();
    public static string GetPlayers() {
        return string.Join(", ", externalPlayers.Select(kvp => $"{kvp.Key}"));
    }

    private static CoinsMap nextMap = CoinsMap.None;
    private static CoinsMap currentMap = CoinsMap.None;
    private GameObject[] respawnPoints;

    void Unload() {
        ObjectPool<Projectile>.Reset();
        GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>().Delete();
        foreach (var player in externalPlayers.Values) {
            player.Delete();
            externalPlayers = new Dictionary<byte, ExternalPlayerController>();
        }
        ConnectionManager.Disconnect();
        Coins = 0;
    }

    public static void Initialize() {
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
        PauseMenuController.Initialize();
    }

    public static void RoundOver(byte retrievedCoins, byte winnerId) {
        EarnedCoins += retrievedCoins;
        PlayerController.BlockInputs = true;
        foreach (var player in externalPlayers.Values) {
            player.Frozen = true;
        }
        PauseMenuController.Open();
    }
    public static void NewRound(byte spawnOffset) {
        PlayerController.BlockInputs = false;
        foreach (var player in externalPlayers.Values) {
            player.Frozen = false;
        }
        Instance?.Respawn(spawnOffset);
    }

    public void Respawn(byte spawnOffset) {
        if (respawnPoints == null || respawnPoints.Length == 0 || playerId == null) {
            Debug.LogWarning("Respawn failed: No respawn points found or playerId is null.");
            return;
        }

        Array.Sort(respawnPoints, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        int index = (playerId.Value + spawnOffset) % respawnPoints.Length;
        Vector3 respawnPos = respawnPoints[index].transform.position;

        PlayerController.SetPosition(respawnPos.x, respawnPos.y);
        PlayerController.Instance.IsDead = false;
        PlayerController.Instance.Refresh();
    }

    public static void CheckMap(byte mapIndex) {
        if (playerId != null) {
            CoinsMap map = System.Enum.IsDefined(typeof(CoinsMap), (int)mapIndex) ? (CoinsMap)(int)mapIndex : CoinsMap.None;
            if (nextMap != map) {
                nextMap = map;
                Debug.Log(map.ToString());
                SceneTransition.LoadScene(map.ToString());
            }
        }
    }

    public void UpdateMap(string mapName) {
        currentMap = GetMapFromString(mapName);
        if (currentMap.Equals(CoinsMap.None)) {
            PauseMenuController.enabled = false;
            Unload();
        } else {
            PauseMenuController.enabled = true;
        }
        nextMap = currentMap;
        Dispatcher.Enqueue(() => {respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");});
        PlayerController.BlockInputs = false;
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
        if (Instance == null) return;
        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data);

        if (playerId == null || currentMap == CoinsMap.None) return;
        //parsing received data
        byte id = span[index + PlayerPacker.PacketLength - 1];
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

        //weapon
        float direction = MemoryMarshal.Read<float>(span.Slice(index)); index += 4;
        int reloadTime = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int maxAmmoCount = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int ammoCount = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int timeBetweenShots = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int burstSize = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int burstTimeBetweenShots = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;
        int attackCount = MemoryMarshal.Read<int>(span.Slice(index)); index += 4;

        ExternalPlayerController player;
        if (!externalPlayers.TryGetValue(id, out player)) {
            GameObject newPlayer = GameObject.Instantiate(Instance.externalPlayerPrefab, position, Quaternion.identity);
            player = newPlayer.GetComponent<ExternalPlayerController>();
            if (player == null || newPlayer == null) {
                return;
            }
            player.playerId = id;
            externalPlayers.Add(id, player);
            player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
            player.SetPosition(position);
        }

        // Update the player's properties using available public setters.
        player.Health = health;
        player.MassPerSize = massPerSize;
        player.MassMultiplier = massMultiplier;
        player.BaseSize = baseSize;
        player.MaxHealth = maxHealth;
        player.MaxHealth_SizeMultiplier = maxHealth_SizeMultiplier;
        player.Color = color;

        //weapon
        player.Weapon.reloadTime = reloadTime;
        player.Weapon.MaxAmmoCount = maxAmmoCount;
        player.Weapon.AmmoCount = ammoCount;
        player.Weapon.timeBetweenShots = timeBetweenShots;
        player.Weapon.burstSize = burstSize;
        player.Weapon.burstTimeBetweenShots = burstTimeBetweenShots;
        player.Weapon.attackCount = attackCount;
        player.Weapon.Aim(direction);

        // Update position and velocity.
        player.UpdatePosition(position);
        player.UpdateVelocity(velocity);
        player.UpdateForce(force);

        player.IsDead = isDead;
    }

    public static void ShotsFired(byte[] data, int index, int count, byte playerId) {
        if (externalPlayers.TryGetValue(playerId, out ExternalPlayerController player)) {
            for (int i = 0; i < count; i++) {
                float x = BitConverter.ToSingle(data, index); index += 4;
                float y = BitConverter.ToSingle(data, index); index += 4;
                float rotation = BitConverter.ToSingle(data, index); index += 4;
                int lifeTime = BitConverter.ToInt32(data, index); index += 4;
                float velocity = BitConverter.ToSingle(data, index); index += 4;
                float acceleration = BitConverter.ToSingle(data, index); index += 4;
                float gravity = BitConverter.ToSingle(data, index); index += 4;
                float knockback = BitConverter.ToSingle(data, index); index += 4;
                int damage = BitConverter.ToInt32(data, index); index += 4;
                int projectileId = BitConverter.ToInt32(data, index); index += 4;

                player.Weapon.Attack(playerId, x, y, rotation, lifeTime, velocity, acceleration, gravity, knockback, damage, projectileId);
            }
        }
    }

    public static void RegisterHits(byte[] data, int index, int count) {
        for (int i = 0; i < count; i++) {
            float kbx = BitConverter.ToSingle(data, index); index += 4;
            float kby = BitConverter.ToSingle(data, index); index += 4;
            Vector2 knockback = new Vector2(kbx, kby);
            int damage = BitConverter.ToInt32(data, index); index += 4;
            byte fromPlayerId = data[index++];
            int projectileId = BitConverter.ToInt32(data, index); index += 4;
            byte toPlayerId = data[index++];

            if (GameController.playerId == null) Debug.LogWarning("GameController.playerId is NULL");
            if (ProjectileRegistry.RegisteredProjectiles.TryGetValue((fromPlayerId, projectileId), out Projectile projectile)) {
                projectile.RegisterHit(knockback, damage, fromPlayerId, toPlayerId, playerId.Value);
            }
        }
    }

    public static void CheckPlayers(byte[] data, int index, byte count) {
        ReceivedPlayerList = new byte[count];
        Array.Copy(data, index, ReceivedPlayerList, 0, count);

        bool[] receivedIds = new bool[256];

        for (int i = 0; i < count; i++) {
            if (playerId != null) if (data[index + i] == playerId.Value) continue;
            receivedIds[data[index + i]] = true;
        }

        List<byte> keysToRemove = null;
        foreach (var kvp in externalPlayers) {
            if (kvp.Value == null) {
                externalPlayers.Remove(kvp.Key);
            }
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
