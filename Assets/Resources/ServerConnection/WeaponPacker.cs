using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(WeaponController))]
public class WeaponPacker : MonoBehaviour {
    private WeaponController weapon;

    private static int shotCount = 0;
    private static List<byte> shots = new List<byte>();

    private static int hitCount = 0;
    private static List<byte> hits = new List<byte>();

    void Awake() {
        weapon = GetComponent<WeaponController>();
        if (weapon == null)
            Debug.LogError("WeaponController component is missing!");
    }

    //Packet structure: NO ID bc its part of the player
    //4 bytes - float direction
    //4 bytes - int reloadTime
    //4 bytes - int maxAmmoCount
    //4 bytes - int ammoCount
    //4 bytes - int timeBetweenShots
    //4 bytes - int burstSize
    //4 bytes - int burstTimeBetweenShots
    //4 bytes - int attackCount

    //32

    public void GetPacket(List<byte> packet) {
        packet.AddRange(BitConverter.GetBytes(weapon.Direction));
        packet.AddRange(BitConverter.GetBytes(weapon.ReloadTime));
        packet.AddRange(BitConverter.GetBytes(weapon.MaxAmmoCount));
        packet.AddRange(BitConverter.GetBytes(weapon.AmmoCount));
        packet.AddRange(BitConverter.GetBytes(weapon.TimeBetweenShots));
        packet.AddRange(BitConverter.GetBytes(weapon.BurstSize));
        packet.AddRange(BitConverter.GetBytes(weapon.BurstTimeBetweenShots));
        packet.AddRange(BitConverter.GetBytes(weapon.AttackCount));
    }

    //Shots packet structure:
    //1 byte - id
    //4 bytes - int count of shots
    //then for each shot:
    //8 bytes - vec2 position
    //4 bytes - float rotation
    //4 bytes - int lifeTime
    //4 bytes - float velocity
    //4 bytes - float acceleration
    //4 bytes - float gravity
    //4 bytes - float knockback
    //4 bytes - int damage
    //4 bytes - int projectileId

    //1 + 4 + 40x
    public static void AddShot(float x, float y, float rotation, int lifeTime, float velocity, float acceleration, float gravity, float knockback, int damage, int projectileId) {
        shots.AddRange(BitConverter.GetBytes(x));
        shots.AddRange(BitConverter.GetBytes(y));
        shots.AddRange(BitConverter.GetBytes(rotation));
        shots.AddRange(BitConverter.GetBytes(lifeTime));
        shots.AddRange(BitConverter.GetBytes(velocity));
        shots.AddRange(BitConverter.GetBytes(acceleration));
        shots.AddRange(BitConverter.GetBytes(gravity));
        shots.AddRange(BitConverter.GetBytes(knockback));
        shots.AddRange(BitConverter.GetBytes(damage));
        shots.AddRange(BitConverter.GetBytes(projectileId));
        shotCount++;
    }
    public static int PerShotPacketSize = 40;
    public static void GetShotsPacket(List<byte> packet) {
        if (shotCount > 0) {
            packet.Add(4); //id
            packet.AddRange(BitConverter.GetBytes(shotCount));
            packet.AddRange(shots);
            shotCount = 0;
            shots.Clear();
        }
    }

    //Hits packet structure:
    //1 byte - id
    //4 bytes - int count of hits
    //then for each hit:
    //8 bytes - vec2 knockback
    //4 bytes - int damage
    //1 byte - int fromPlayerId
    //4 bytes - int projectile id
    //1 byte - int toPlayerId

    //1 + 4 + 18x
    public static int PerHitPacketSize = 18;
    public static void AddHit(Vector2 knockback, int damage, byte fromPlayerId, int projectileId, byte toPlayerId) {
        hits.AddRange(BitConverter.GetBytes(knockback.x));
        hits.AddRange(BitConverter.GetBytes(knockback.y));
        hits.AddRange(BitConverter.GetBytes(damage));
        hits.Add(fromPlayerId);
        hits.AddRange(BitConverter.GetBytes(projectileId));
        hits.Add(toPlayerId);
        hitCount++;
    }
    public static void GetHitsPacket(List<byte> packet) {
        if (hitCount > 0) {
            packet.Add(5); //id
            packet.AddRange(BitConverter.GetBytes(hitCount));
            packet.AddRange(hits);
            hitCount = 0;
            hits.Clear();
        }
    }
}
