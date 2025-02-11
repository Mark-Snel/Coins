using UnityEngine;
using static InputManager;

public class PlayerController : MonoBehaviour
{
    // --- Player Stats
    [Header("Movement Stats")]
    public float speed = 7f;           // Base horizontal speed
    public float acceleration = 12f;   // How quickly force ramps up
    public float jumpHeight = 40f;     // (After square-rooting it, used as jump impulse)
    public int maxJumps = 1;           // Number of jumps allowed (e.g. for double-jump)
    public float massFactor = 0.25f;   // Player mass used in calculations (and applied to Rigidbody2D)

    // --- Private Variables ---
    private Rigidbody2D rb;
    private int jumpsRemaining;        // Tracks how many jumps remain before landing
    private int jumpLength;            // Frames remaining for “hold jump” extra force

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Optionally enforce our mass setting (make sure your Rigidbody2D is set to Dynamic)
        rb.mass = massFactor;

        // Initialize jumps
        jumpsRemaining = maxJumps;
    }

    // Use FixedUpdate for physics-related updates.
    void FixedUpdate()
    {
        ProcessJump();
        ProcessHorizontalMovement();
    }

    /// <summary>
    /// Handles the jump input.
    ///
    /// The original JS logic was:
    /// - On initial press (keyState >= 2 or if held while not having jumped yet) and if jumps remain:
    ///     • Set the vertical velocity to sqrt(jumpHeight) (note: in the JS code negative was upward, so here we use positive)
    ///     • Deduct one jump and set jumpLength = 10 (i.e. allow a few frames of “jump hold”).
    /// - Otherwise, if the jump is still held and jumpLength > 0, apply an extra upward force.
    /// </summary>
    void ProcessJump()
    {
        // (In the original code the condition was:
        //   if ((keyState >= 2 && jumpsRemaining > 0) || (keyState == 1 && jumpsRemaining == maxJumps))
        // Here, we treat GetKeyDown as "pressed" (keyState 2) and GetKey as "held".)
        KeyState ActionState = Inputs.GetActionState(Action.Jump);
        if ((ActionState == KeyState.Pressed && jumpsRemaining > 0) ||
            (ActionState == KeyState.Held && jumpsRemaining == maxJumps))
        {
            // Initial jump: set the vertical velocity.
            float jumpImpulse = Mathf.Sqrt(jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);

            // Consume one jump.
            jumpsRemaining--;
            // Allow additional force for up to 10 physics frames if the key is held:
            jumpLength = 10;
            // (The original code also set a jumpCooldown, but that isn’t used elsewhere.)
        }
        // Else, if the player is still holding jump and we have “hold time” remaining, apply extra upward force.
        else if (jumpLength > 0 && ActionState == KeyState.Held)
        {
            // The original JS applied:
            //   force = Math.sqrt(jumpHeight) * -2 * mass  (with negative meaning “up” in its coordinate system)
            // In Unity (where up is positive) we use a positive force.
            float holdForce = Mathf.Sqrt(jumpHeight) * 2f * rb.mass;
            rb.AddForce(Vector2.up * holdForce);
            jumpLength--;  // decrement the hold time counter
        }
    }

    /// <summary>
    /// Processes left/right input.
    /// </summary>
    void ProcessHorizontalMovement()
    {
        KeyState LeftActionState = Inputs.GetActionState(Action.Left);
        KeyState RightActionState = Inputs.GetActionState(Action.Right);
        float currentVelocityX = rb.linearVelocity.x;

        // Process left movement:
        if (LeftActionState == KeyState.Pressed || LeftActionState == KeyState.Held)
        {
            // Replicating the JS:
            //   forceX = Math.min( Math.min( Math.max(v * -2 - speed*2, -acceleration * mass),
            //                                  Math.max(v * -acceleration - acceleration*mass, -2*acceleration*mass) ),
            //                      0 );
            float forceLeft1 = Mathf.Max(currentVelocityX * -2f - speed * 2f, -acceleration * rb.mass);
            float forceLeft2 = Mathf.Max(currentVelocityX * -acceleration - acceleration * rb.mass, -2f * acceleration * rb.mass);
            float computedForceX = Mathf.Min(Mathf.Min(forceLeft1, forceLeft2), 0f);

            rb.AddForce(new Vector2(computedForceX, 0f));
        }

        // Process right movement:
        if (RightActionState == KeyState.Pressed || RightActionState == KeyState.Held)
        {
            // Replicating the JS:
            //   forceX = Math.max( Math.max( Math.min(v * -2 + speed*2, acceleration * mass),
            //                                  Math.min(v * -acceleration + acceleration*mass, 2*acceleration*mass) ),
            //                      0 );
            float forceRight1 = Mathf.Min(currentVelocityX * -2f + speed * 2f, acceleration * rb.mass);
            float forceRight2 = Mathf.Min(currentVelocityX * -acceleration + acceleration * rb.mass, 2f * acceleration * rb.mass);
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
                jumpsRemaining = maxJumps;
                jumpLength = 0;
                break; // Exit loop after first valid contact
            }
        }
    }
}
