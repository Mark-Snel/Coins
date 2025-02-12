using UnityEngine;
using UnityEngine.InputSystem;
using static InputHandling;

public class PlayerController : MonoBehaviour
{
    // --- Player Stats
    [Header("Player Stats")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float jumpHeight = 12f;
    [SerializeField] private int jumpLength = 12;
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private float mass = 0.2f;
    [SerializeField] private float baseSize = 0.1f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float maxHealth_SizeMultiplier = 0.01f;

    public float Speed { get { return speed; } set { speed = value; } }
    public float Acceleration { get { return acceleration; } set { acceleration = value; } }
    public float JumpHeight { get { return jumpHeight; } set { jumpHeight = value; } }
    public int JumpLength { get { return jumpLength; } set { jumpLength = value; } }
    public int MaxJumps { get { return maxJumps; } set { maxJumps = value; } }
    public float Mass
    {
        get { return mass; }
        set
        {
            if (mass != value)
            {
                mass = value;
                rb.mass = mass;
            }
        }
    }
    public float BaseSize
    {
        get { return baseSize; }
        set
        {
            if (baseSize != value)
            {
                baseSize = value;
                UpdateSize();
            }
        }
    }
    public int MaxHealth
    {
        get { return maxHealth; }
        set
        {
            if (maxHealth != value)
            {
                maxHealth = value;
                UpdateSize();
            }
        }
    }
    public float MaxHealth_SizeMultiplier
    {
        get { return maxHealth_SizeMultiplier; }
        set
        {
            if (maxHealth_SizeMultiplier != value)
            {
                maxHealth_SizeMultiplier = value;
                UpdateSize();
            }
        }
    }

    // --- Private Variables ---
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private CircleCollider2D cl;
    private InputAction jumpAction;
    private KeyState jumpState = KeyState.None;
    private InputAction moveAction;
    private int jumpsRemaining;        // Tracks how many jumps remain before landing
    private int jumpLengthRemaining;            // Frames remaining for “hold jump” extra force
    private int health;
    private float size;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        cl = GetComponent<Collider2D>() as CircleCollider2D;
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        rb.mass = mass;
        health = maxHealth;
        UpdateSize();
        jumpsRemaining = MaxJumps;
    }

    void Update() {
        UpdateKeyState(jumpAction, ref jumpState);
    }

    // Use FixedUpdate for physics-related updates.
    void FixedUpdate()
    {
        ProcessJump();
        ProcessHorizontalMovement();
        jumpState = KeyState.None;
    }

    void ProcessJump()
    {
        if (jumpState == KeyState.Pressed && jumpsRemaining > 0)
        {
            float jumpImpulse = Mathf.Sqrt(jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);

            jumpsRemaining--;
            jumpLengthRemaining = jumpLength;
        }
        // Else, if the player is still holding jump and we have “hold time” remaining, apply extra upward force.
        if (jumpLengthRemaining > 0 && jumpState == KeyState.Held)
        {
            float holdForce = Mathf.Sqrt(jumpHeight) * 2f * rb.mass;
            rb.AddForce(Vector2.up * holdForce);
            jumpLengthRemaining--;  // decrement the hold time counter
        }
    }

    /// <summary>
    /// Processes left/right input.
    /// </summary>
    void ProcessHorizontalMovement()
    {
        float currentVelocityX = rb.linearVelocity.x;
        float input = moveAction.ReadValue<float>();
        Debug.Log(input);
        if (input < 0)
        {
            float forceLeft1 = Mathf.Max(currentVelocityX * -2f + input * speed * 2f, input * acceleration * mass);
            float forceLeft2 = Mathf.Max(currentVelocityX * -acceleration + input * acceleration * mass, input * 2f * acceleration * mass);
            float computedForceX = Mathf.Min(Mathf.Min(forceLeft1, forceLeft2), 0f);
            rb.AddForce(new Vector2(computedForceX, 0f));
        }

        if (input > 0)
        {
            float forceRight1 = Mathf.Min(currentVelocityX * -2f + input * speed * 2f, input * acceleration * mass);
            float forceRight2 = Mathf.Min(currentVelocityX * -acceleration + input * acceleration * mass, input * 2f * acceleration * mass);
            float computedForceX = Mathf.Max(Mathf.Max(forceRight1, forceRight2), 0f);
            rb.AddForce(new Vector2(computedForceX, 0f));
        }
    }

    /// <summary>
    /// When colliding with ground, reset available jumps.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            float angle = Vector2.Angle(normal, Vector2.up); // Angle between normal and "up" (0° is perfectly flat ground)

            if (angle <= 45f) // Allow jumps only if the slope is <= 45 degrees
            {
                jumpsRemaining = MaxJumps;
                jumpLengthRemaining = 0;
                break; // Exit loop after first valid contact
            }
        }
    }

    private void UpdateSize()
    {
        size = baseSize + (maxHealth_SizeMultiplier * Mathf.Sqrt(maxHealth));
        //cl.radius = size; // Apparently this happens automatically if you execute the line below, this would double it
        sr.transform.localScale = new Vector3(size, size, 1f);

    }
}
