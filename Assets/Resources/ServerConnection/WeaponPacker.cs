using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(WeaponController))]
public class WeaponPacker : MonoBehaviour {
    private WeaponController weapon;

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
}
