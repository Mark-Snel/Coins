using UnityEngine;
using UnityEngine.InputSystem;
using static InputHandling;
using static ColorManager;

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance { get; private set; }
    public static bool BlockInputs = false;

    [SerializeField] private bool isDead = true;
    [SerializeField] private int color = 11;
    [Header("Player Stats")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float jumpHeight = 12f;
    [SerializeField] private int jumpLength = 12;
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private float massPerSize = 1f;
    [SerializeField] private float massMultiplier = 1f;
    [SerializeField] private float baseSize = 0.1f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float maxHealth_SizeMultiplier = 0.01f;
    [SerializeField] private int health;

    private float edgeSize = 0.04f;

    public int Color { get { return color; } set {color = value;} }
    public float Speed { get { return speed; } set { speed = value; } }
    public float Acceleration { get { return acceleration; } set { acceleration = value; } }
    public float JumpHeight { get { return jumpHeight; } set { jumpHeight = value; } }
    public int JumpLength { get { return jumpLength; } set { jumpLength = value; } }
    public int MaxJumps { get { return maxJumps; } set { maxJumps = value; } }
    public float MassPerSize {
        get { return massPerSize; }
        set {
            if (massPerSize != value) {
                massPerSize = value;
                UpdateSizeAndMass();
            }
        }
    }
    public float MassMultiplier {
        get { return massMultiplier; }
        set {
            if (massMultiplier != value) {
                massMultiplier = value;
                UpdateSizeAndMass();
            }
        }
    }
    public float BaseSize {
        get { return baseSize; }
        set {
            if (baseSize != value) {
                baseSize = value;
                UpdateSizeAndMass();
            }
        }
    }
    public int MaxHealth {
        get { return maxHealth; }
        set {
            if (maxHealth != value) {
                maxHealth = value;
                hud?.UpdateHealth(health, maxHealth);
                UpdateSizeAndMass();
            }
        }
    }
    public float MaxHealth_SizeMultiplier {
        get { return maxHealth_SizeMultiplier; }
        set {
            if (maxHealth_SizeMultiplier != value) {
                maxHealth_SizeMultiplier = value;
                UpdateSizeAndMass();
            }
        }
    }
    public bool IsDead {
        get { return isDead; }
        set {
            if (isDead != value) {
                isDead = value;
                ProcessDeath();
            }
        }
    }
    public int Health {
        get { return health; }
        set {
            if (health != value) {
                health = value;
                hud?.UpdateHealth(health, maxHealth);
                if (health <= 0) {
                    IsDead = true;
                }
            }
        }
    }

    private static PlayerPacker dataPacker;
    private static WeaponPacker weaponPacker;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform ist;
    private SpriteRenderer isr;
    private CircleCollider2D cl;
    private InputAction jumpAction;
    private KeyState jumpState = KeyState.None;
    private InputAction moveAction;
    private int jumpsRemaining; // Tracks how many jumps remain before landing
    private int jumpLengthRemaining; //Ticks remaining for “hold jump” extra force
    private float size;

    private InputAction attackAction;
    private InputAction aimAction;
    [SerializeField] public WeaponController Weapon;

    private HUD hud;
    public void UpdateMaxAmmo(int count){
        hud?.UpdateMaxAmmo(count);
    }
    public void UpdateAmmo(int count) {
        hud?.UpdateAmmo(count);
    }
    public void UpdateReload(int reloadTime, int reloadProgress) {
        hud?.UpdateReload(reloadTime, reloadProgress);
    }

    public static void SetPosition(float x, float y) {
        Instance.rb.position = new Vector2(x, y);
        Instance.transform.position = new Vector3(x, y, Instance.transform.position.z);
    }

    public void Delete() {
        Instance = null;
        dataPacker = null;
        Destroy(gameObject);
    }

    public static WeaponPacker GetWeaponPacker() {
        return weaponPacker;
    }

    public static PlayerPacker GetDataPacker() {
        return dataPacker;
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        hud = transform.Find("HUD").GetComponent<HUD>();
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        Application.runInBackground = true;
        dataPacker = GetComponent<PlayerPacker>();
        weaponPacker = Weapon?.GetComponent<WeaponPacker>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ist = transform.Find("InnerSprite");
        if (ist != null) {
            isr = ist.GetComponent<SpriteRenderer>();
            if (isr == null) {
                Debug.LogError("Invalid InnerSprite in player");
            }
        } else {
            Debug.LogError("InnerSprite not found");
        }
        cl = GetComponent<CircleCollider2D>();

        moveAction = InputSystem.actions.FindAction("Move");
        if (moveAction == null) {
            Debug.LogWarning("moveAction not found");
        }
        jumpAction = InputSystem.actions.FindAction("Jump");
        if (jumpAction == null) {
            Debug.LogWarning("jumpAction not found");
        }
        attackAction = InputSystem.actions.FindAction("Attack");
        if (attackAction == null) {
            Debug.LogWarning("attackAction not found");
        }
        aimAction = InputSystem.actions.FindAction("Aim");
        if (attackAction == null) {
            Debug.LogWarning("aimAction not found");
        }

        Health = maxHealth;
        UpdateSizeAndMass();
        Color = ColorManager.GetColor();
        UpdateColor();
        jumpsRemaining = maxJumps;
        ProcessDeath();
    }

    void Update() {
        if (!isDead && !BlockInputs) {
            UpdateKeyState(jumpAction, ref jumpState);
            Weapon?.Attack(
                GetRotationToCursor(transform.position, aimAction, Camera.main),
                GetKeyState(attackAction)
            );
        }
    }

    void FixedUpdate() {
        if (BlockInputs) {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            return;
        } else {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        if (!isDead) {
            ProcessJump();
            ProcessHorizontalMovement();
            if (health <= 0) {
                IsDead = true;
            }
        }
        jumpState = KeyState.None;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        ProcessJumpReset(collision);
    }
    private void OnCollisionStay2D(Collision2D collision) {
        ProcessJumpReset(collision);
    }

    void ProcessJump() {
        if ((jumpState == KeyState.Pressed && jumpsRemaining > 0) || (jumpState == KeyState.Held && jumpsRemaining == maxJumps))
        {
            float jumpImpulse = Mathf.Sqrt(jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(jumpImpulse, rb.linearVelocity.y));

            jumpsRemaining--;
            jumpLengthRemaining = jumpLength;
        }
        //if the player is still holding jump and we have “hold time” remaining, apply extra upward force.
        if (jumpLengthRemaining > 0 && jumpState == KeyState.Held)
        {
            float holdForce = Mathf.Sqrt(jumpHeight) * 2f * rb.mass;
            rb.AddForce(Vector2.up * holdForce);
            jumpLengthRemaining--;  // decrement the hold time counter
        }
    }

    void ProcessJumpReset(Collision2D collision) {
        foreach (ContactPoint2D contact in collision.contacts) {
            Vector2 normal = contact.normal;
            float angle = Vector2.Angle(normal, Vector2.up); //Angle between normal and "up" (0° is perfectly flat ground)

            if (angle <= 45f) { //Allow jumps only if the slope is <= 45 degrees
                jumpsRemaining = maxJumps;
                jumpLengthRemaining = 0;
                break;
            }
        }
    }

    void ProcessHorizontalMovement() {
        //Dont mind the jankyness of this code, it is to revive a dead project and keep the feel the same.
        float currentVelocityX = rb.linearVelocity.x;
        float input = moveAction.ReadValue<float>();
        if (input < 0) {
            float forceLeft1 = Mathf.Max(currentVelocityX * -2f + input * speed * 2f, input * acceleration);
            float forceLeft2 = Mathf.Max(currentVelocityX * -acceleration + input * acceleration, input * 2f * acceleration);
            float computedForceX = Mathf.Min(Mathf.Min(forceLeft1, forceLeft2), 0f);
            ApplyForce(new Vector2(computedForceX * rb.mass, 0f));
        }

        if (input > 0) {
            float forceRight1 = Mathf.Min(currentVelocityX * -2f + input * speed * 2f, input * acceleration);
            float forceRight2 = Mathf.Min(currentVelocityX * -acceleration + input * acceleration, input * 2f * acceleration);
            float computedForceX = Mathf.Max(Mathf.Max(forceRight1, forceRight2), 0f);
            ApplyForce(new Vector2(computedForceX * rb.mass, 0f));
        }
    }

    private void UpdateColor() {
        Weapon?.SetColor(color);
        sr.color = GetColor(color).Secondary;
        isr.color = GetColor(color).Primary;
    }

    private void UpdateSizeAndMass() {
        if (health > maxHealth) Health = MaxHealth;
        size = baseSize + (maxHealth_SizeMultiplier * Mathf.Sqrt(maxHealth));
        transform.localScale = new Vector3(size, size, 1f);
        float innerSize = (size - edgeSize) / size;
        // ist.localScale = new Vector3(innerSize, innerSize, 1f);
        rb.mass = massPerSize * size * size * massMultiplier;
    }

    private void ProcessDeath() {
        if (isDead) {
            hud?.SetActive(false);
            Weapon?.gameObject.SetActive(false);
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
            cl.enabled = false;
            sr.enabled = false;
            isr.enabled = false;
        } else {
            hud?.SetActive(true);
            Weapon?.gameObject.SetActive(true);
            Health = MaxHealth;
            rb.simulated = true;
            cl.enabled = true;
            sr.enabled = true;
            isr.enabled = true;
        }
    }

    private void OnValidate() {
        if (rb) {
            UpdateSizeAndMass();
            UpdateColor();
            ProcessDeath();
        }
    }

    public void Refresh() {
        Weapon?.Reload();
        Health = MaxHealth;
    }

    public void Hit(Vector2 knockback, int damage, byte fromPlayerId, int projectileId) {
        Health -= damage;
        rb.AddForce(knockback);
        if (GameController.playerId == null) Debug.LogWarning("GameController.playerId is NULL");
        WeaponPacker.AddHit(knockback, damage, fromPlayerId, projectileId, GameController.playerId.Value);
    }
    public void Hit(Vector2 knockback, int damage) {
        Health -= damage;
        rb.AddForce(knockback);
        if (GameController.playerId == null) Debug.LogWarning("GameController.playerId is NULL");
    }

    private Vector2 totalForce = Vector2.zero;
    private void ApplyForce(Vector2 force) {
        rb.AddForce(force);
        totalForce += force;
    }

    public Vector2 GetForce() {
        Vector2 returnForce = totalForce;
        totalForce = Vector2.zero;
        return returnForce;
    }
}
