using UnityEngine;

public class Launcher : MonoBehaviour
{
    public Vector2 launchVector;
    void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null) {
            rb.linearVelocity = launchVector;
        }
    }
}
