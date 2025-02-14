using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Vector3 DynamicOffset = new Vector3(0, 0.5f, 0);
    public Vector2 DynamicBoundary = Vector2.zero; //max width and height for the camera to travel to, should be set to the total width and height of the 'map'
    public float smoothSpeed = 0.1f;
    public float lookAheadFactor = 0.2f;
    public float maxLookAhead = 3f;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerController playerController;
    void Start() {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                rb = foundPlayer.GetComponent<Rigidbody2D>();
                playerController = foundPlayer.GetComponent<PlayerController>();
                if (rb == null || playerController == null) {
                    Debug.LogError("Invalid player");
                }
            }
            else
            {
                Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
            }
        }
    }

    void LateUpdate() {
        if (player != null && !playerController.IsDead) {
            Vector3 lookAheadOffset = Vector3.ClampMagnitude((Vector3)rb.linearVelocity * lookAheadFactor, maxLookAhead);
            Vector3 desiredPosition = player.position + lookAheadOffset + DynamicOffset;
            float clampedX = Mathf.Clamp(desiredPosition.x, -DynamicBoundary.x / 2, DynamicBoundary.x / 2);
            float clampedY = Mathf.Clamp(desiredPosition.y, -DynamicBoundary.y / 2, DynamicBoundary.y / 2);
            transform.position = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);
        } else {
            transform.position = Vector3.Lerp(transform.position, Vector3.zero, smoothSpeed);
        }
    }
}
