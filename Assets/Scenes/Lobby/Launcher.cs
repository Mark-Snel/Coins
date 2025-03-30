using UnityEngine;

public class Launcher : MonoBehaviour
{
    public Vector2 launchVector;
    public bool keepVelocity = false;
    void OnCollisionEnter2D(Collision2D collision) {
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null) {
            if (keepVelocity) {
                rb.linearVelocity += launchVector;
            } else {
                rb.linearVelocity = launchVector;
            }
        }
    }
}
