using UnityEngine;
using static ColorManager;

public class ExternalPlayerController : MonoBehaviour
{
    [SerializeField] private bool isDead = true;
    [SerializeField] private int color = 11;
    [Header("Player Stats")]
    [SerializeField] private float massPerSize = 1f;
    [SerializeField] private float massMultiplier = 1f;
    [SerializeField] private float baseSize = 0.1f;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float maxHealth_SizeMultiplier = 0.01f;
    [SerializeField] private int health;
    [SerializeField] private float interpolationFactor = 0.2f;
    public int playerId;

    private float edgeSize = 0.04f;

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
                if (health <= 0) {
                    IsDead = true;
                }
            }
        }
    }
    public int Color {
        get { return color; }
        set {
            if (color != value) {
                color = value;
                UpdateColor();
            }
        }
    }

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform ist;
    private SpriteRenderer isr;
    private CircleCollider2D cl;
    private float size;

    [SerializeField] private ExternalWeaponController weapon;
    public ExternalWeaponController Weapon {
        get {return weapon;}
        private set {}
    }

    public HUD hud;
    public void UpdateMaxAmmo(int count){
        hud.UpdateMaxAmmo(count);
    }
    public void UpdateAmmo(int count) {
        hud.UpdateAmmo(count);
    }
    public void UpdateReload(int reloadTime, int reloadProgress) {
        hud.UpdateReload(reloadTime, reloadProgress);
    }

    void Start() {
        DontDestroyOnLoad(gameObject);
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ist = transform.Find("InnerSprite"); // Replace with actual name
        if (ist != null) {
            isr = ist.GetComponent<SpriteRenderer>();
            if (isr == null) {
                Debug.LogError("Invalid InnerSprite in player");
            }
        } else {
            Debug.LogError("InnerSprite not found");
        }
        cl = GetComponent<CircleCollider2D>();

        health = maxHealth;
        UpdateSizeAndMass();
        UpdateColor();
        ProcessDeath();
        if (targetPosition != null) {
            rb.position = targetPosition;
        }
    }

    private void UpdateColor() {
        weapon?.SetColor(color);
        if (sr) {
            sr.color = GetColor(color).Secondary;
            isr.color = GetColor(color).Primary;
        }
    }

    private void UpdateSizeAndMass() {
        if (health > maxHealth) health = MaxHealth;
        size = baseSize + (maxHealth_SizeMultiplier * Mathf.Sqrt(maxHealth));
        transform.localScale = new Vector3(size, size, 1f);
        float innerSize = (size - edgeSize) / size;
        ist.localScale = new Vector3(innerSize, innerSize, 1f);
        rb.mass = massPerSize * size * size * massMultiplier;
    }

    private Vector2 velocity = Vector2.zero;
    private void FixedUpdate() {
        if (targetPosition != null) {
            Vector2 smoothPosition = Vector2.SmoothDamp(rb.position, targetPosition, ref velocity, interpolationFactor);
            rb.position = smoothPosition;
        }
        rb.AddForce(appliedForce);
    }

    private void ProcessDeath() {
        if (rb && cl) {
            if (isDead) {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
                cl.enabled = false;
                sr.enabled = false;
                isr.enabled = false;
            } else {
                health = MaxHealth;
                rb.simulated = true;
                cl.enabled = true;
                sr.enabled = true;
                isr.enabled = true;
            }
        }
    }

    private void OnValidate() {
        if (rb) {
            UpdateSizeAndMass();
            UpdateColor();
            ProcessDeath();
        }
    }

    public void UpdateVelocity(Vector2 velocity) {
        if (rb) {
            rb.linearVelocity = velocity;
        }
    }

    private Vector2 targetPosition;
    public void UpdatePosition(Vector2 position) {
        targetPosition = position;
    }

    public void SetPosition(Vector2 position) {
        rb = GetComponent<Rigidbody2D>();
        rb.position = position;
    }


    private Vector2 appliedForce = Vector2.zero;
    public void UpdateForce(Vector2 force) {
        if (rb) {
            appliedForce = force;
        }
    }

    public void Delete() {
        Destroy(gameObject);
    }
}
