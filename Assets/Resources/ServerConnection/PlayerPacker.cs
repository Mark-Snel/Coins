using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class PlayerPacker : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerController player;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();

        if (rb == null)
            Debug.LogError("Rigidbody2D component is missing!");

        if (player == null)
            Debug.LogError("PlayerController component is missing!");
    }

    //Packet structure:
    //1 byte  - bool isDead
    //4 bytes - int color
    //4 bytes - int health
    //4 bytes - float massPerSize
    //4 bytes - float massMultiplier
    //4 bytes - float baseSize
    //4 bytes - int maxHealth
    //4 bytes - float maxHealth_SizeMultiplier
    //8 bytes - Vector2 position (x, y)
    //8 bytes - Vector2 velocity (x, y)
    //8 bytes - Vector2 force (x, y)

    //Total = 1 + 7*4 + 8*3 = 53 bytes
    //Also identifier, so +1 = 54 bytes

    //On server theres also (added to the back):
    //1 byte - byte id
    //Which would be 54 bytes received (without identifier)
    public static int PacketLength { get; private set; } = 54;

    public void GetPacket(List<byte> packet) {
        //Identifier
        packet.Add(3); //Look at UdpConnection, explains each identifier

        //Pack bool isDead as a single byte (0 = false, 1 = true)
        packet.Add(player.IsDead ? (byte)1 : (byte)0);

        //Helper: pack an int (4 bytes)
        packet.AddRange(BitConverter.GetBytes(player.Color));

        //Pack int health
        packet.AddRange(BitConverter.GetBytes(player.Health));

        //Pack float massPerSize
        packet.AddRange(BitConverter.GetBytes(player.MassPerSize));

        //Pack float massMultiplier
        packet.AddRange(BitConverter.GetBytes(player.MassMultiplier));

        //Pack float baseSize
        packet.AddRange(BitConverter.GetBytes(player.BaseSize));

        //Pack int maxHealth
        packet.AddRange(BitConverter.GetBytes(player.MaxHealth));

        //Pack float maxHealth_SizeMultiplier
        packet.AddRange(BitConverter.GetBytes(player.MaxHealth_SizeMultiplier));

        //Pack Rigidbody2D position (Vector2: x and y)
        packet.AddRange(BitConverter.GetBytes(rb.position.x));
        packet.AddRange(BitConverter.GetBytes(rb.position.y));

        //Pack Rigidbody2D velocity (Vector2: x and y)
        packet.AddRange(BitConverter.GetBytes(rb.linearVelocity.x));
        packet.AddRange(BitConverter.GetBytes(rb.linearVelocity.y));

        Vector2 force = player.GetForce();
        packet.AddRange(BitConverter.GetBytes(force.x));
        packet.AddRange(BitConverter.GetBytes(force.y));
    }
}
