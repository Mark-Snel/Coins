using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    public static PlayerCamera Instance;
    
    public Vector3 DynamicOffset = new Vector3(0, 0.5f, 0);
    public Vector2 MapSize = Vector2.zero;
    public Vector2 DefaultPosition = Vector2.zero;
    public float smoothSpeed = 0.07f;
    public float lookAheadFactor = 0.2f;
    public float maxLookAhead = 3f;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerController playerController;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (Instance != this) {
            // Transfer settings to the existing instance
            Instance.DynamicOffset = DynamicOffset;
            Instance.MapSize = MapSize;
            Instance.DefaultPosition = DefaultPosition;
            Instance.smoothSpeed = smoothSpeed;
            Instance.lookAheadFactor = lookAheadFactor;
            Instance.maxLookAhead = maxLookAhead;
            
            Destroy(gameObject);
            return;
        }
    }

    void Start() {
        FindPlayer();
    }

    void FindPlayer() {
        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null) {
            player = foundPlayer.transform;
            rb = foundPlayer.GetComponent<Rigidbody2D>();
            playerController = foundPlayer.GetComponent<PlayerController>();
            if (rb == null || playerController == null) {
                Debug.LogError("Invalid player");
            }
        }
    }

    void FixedUpdate() {
        if (player == null || playerController == null) {
            FindPlayer();
        }
    }

    void LateUpdate() {
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        if (player != null && playerController != null && !playerController.IsDead) {
            Vector3 lookAheadOffset = Vector3.ClampMagnitude((Vector3)rb.linearVelocity * lookAheadFactor, maxLookAhead);
            Vector3 desiredPosition = player.position + lookAheadOffset + DynamicOffset;
            float clampedX = Mathf.Clamp(desiredPosition.x, Mathf.Min((-MapSize.x / 2) + camHalfWidth, 0), Mathf.Max((MapSize.x / 2) - camHalfWidth, 0));
            float clampedY = Mathf.Clamp(desiredPosition.y, Mathf.Min((-MapSize.y / 2) + camHalfHeight, 0), Mathf.Max((MapSize.y / 2) - camHalfHeight, 0));
            transform.position = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);
        } else if (GameController.externalPlayers != null && GameController.externalPlayers.Count > 0) {
            foreach (var externalPlayer in GameController.externalPlayers.Values) {
                if (!externalPlayer.IsDead) {
                    Vector3 desiredPosition = externalPlayer.transform.position + DynamicOffset;
                    float clampedX = Mathf.Clamp(desiredPosition.x, Mathf.Min((-MapSize.x / 2) + camHalfWidth, 0), Mathf.Max((MapSize.x / 2) - camHalfWidth, 0));
                    float clampedY = Mathf.Clamp(desiredPosition.y, Mathf.Min((-MapSize.y / 2) + camHalfHeight, 0), Mathf.Max((MapSize.y / 2) - camHalfHeight, 0));
                    transform.position = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);
                    break; // Only follow the first non-dead external player.
                }
            }
        } else {
            transform.position = Vector3.Lerp(transform.position, new Vector3(DefaultPosition.x, DefaultPosition.y, transform.position.z), smoothSpeed);
        }
    }
}