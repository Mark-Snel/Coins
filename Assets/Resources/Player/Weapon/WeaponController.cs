using UnityEngine;
using static ColorManager;
using static InputHandling;
using static ProjectilePool;

public class WeaponController : MonoBehaviour {
    //weapon behaviour
    [SerializeField] private bool automatic = false;
    [SerializeField] private int reloadTime = 200; //in ticks
    [SerializeField] private int maxAmmoCount = 3;
    public int MaxAmmoCount {
        get { return maxAmmoCount; }
        set {
            if (maxAmmoCount != value) {
                maxAmmoCount = value;
                UpdateMaxAmmo();
            }
        }
    }
    [SerializeField] private int burstSize = 1;
    [SerializeField] private int burstTimeBetweenShots = 5; //only applies if burstSize > 1, in ticks
    [SerializeField] private int timeBetweenShots = 25; //in ticks
    [SerializeField] private float inheritInertia = 0; //multiplier of how much speed the projectile gets from the velocity of the player when shot.
    [SerializeField] private float recoil = 0;

    //projectile behaviour
    [SerializeField] private float spread = 0;
    [SerializeField] private int attackLifeTime = 200; //in ticks
    [SerializeField] private int attackCount = 1; //projectiles per shot, a higher number cause a shotgun-effect
    [SerializeField] private float attackVelocity = 10;
    [SerializeField] private float attackAcceleration = 0;
    [SerializeField] private float attackGravity = 0.5f;
    [SerializeField] private float knockback = 1;
    [SerializeField] private int damage = 30;

    private int ammoCount;
    private int burstRemaining = 0;
    private int burstCooldown = 0;
    private int cooldown = 0;
    private int reloading = 0;

    private Rigidbody2D playerRb;
    private Transform barrel;
    private Animator barrelAnimator;
    private SpriteRenderer baseSprite;
    private SpriteRenderer flashSprite;
    public Sprite[] muzzleFlashSprites;
    private float muzzleFlashTime = 0.02f;//in seconds
    private float flashTimePassed = 0;
    private int color = -2;
    private PlayerController player;

    public float Direction {
        get { return transform.eulerAngles.z; }
        private set {}
    }
    public int ReloadTime { get { return reloadTime; } set { reloadTime = value; } }
    public int AmmoCount { get { return ammoCount; } set { ammoCount = value; } }
    public int TimeBetweenShots { get { return timeBetweenShots; } set { timeBetweenShots = value; } }
    public int BurstSize { get { return burstSize; } set { burstSize = value; } }
    public int BurstTimeBetweenShots { get { return burstTimeBetweenShots; } set { burstTimeBetweenShots = value; } }
    public int AttackCount { get { return attackCount; } set { attackCount = value; } }

    public void SetColor(int color) {
        this.color = color;
        Vector2 hs = GetHueSaturation(color);
        baseSprite.material.SetFloat("_Hue", hs.x);
        baseSprite.material.SetFloat("_Saturation", hs.y);
    }

    void Start() {
        player = transform.parent.GetComponent<PlayerController>();
        baseSprite = transform.Find("Base")?.GetComponent<SpriteRenderer>();
        flashSprite = transform.Find("Flash")?.GetComponent<SpriteRenderer>();
        playerRb = transform.parent.GetComponent<Rigidbody2D>();
        barrel = transform.Find("Barrel");
        barrelAnimator = barrel?.GetComponent<Animator>();

        ammoCount = maxAmmoCount;
        player.UpdateMaxAmmo(maxAmmoCount);
        player.UpdateAmmo(ammoCount);
        burstCooldown = burstTimeBetweenShots;
    }

    void FixedUpdate() {
        if (burstRemaining > 0) {
            burstCooldown--;
            if (burstCooldown <= 0) {
                burstCooldown = burstTimeBetweenShots;
                Fire();
            }
        } else if (cooldown > 0) cooldown--;
        if (ammoCount <= 0 && reloading <= 0) {
            ammoCount = maxAmmoCount;
            player.UpdateAmmo(ammoCount);
            player.UpdateReload(reloadTime, reloading);
        } else if (reloading > 0) {
            reloading--;
            player.UpdateReload(reloadTime, reloading);
        }
    }

    public void Attack(float direction, KeyState keyState) {
        transform.rotation = Quaternion.Euler(0, 0, direction);
        if (Mathf.Abs(direction) > 90) {
            transform.localScale = new Vector3(1, -1, 1);
        } else {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (keyState == KeyState.Pressed || (automatic && keyState == KeyState.Held)) {
            if (ammoCount > 0 && cooldown <= 0) {
                burstRemaining = burstSize;
                burstCooldown = burstTimeBetweenShots;
                Fire();
            }
        }
    }

    void Fire() {
        cooldown = timeBetweenShots;
        burstRemaining--;
        showMuzzleFlash();
        for (int i = 0; i < attackCount; i++) {
            ammoCount--;
            Projectile projectile = ProjectilePool.GetProjectile();
            float weaponRotation = transform.rotation.eulerAngles.z;
            float spreadAngle = Random.Range(-spread / 2f, spread / 2f);
            float projectileRotation = weaponRotation + spreadAngle;
            Vector2 projectileBaseVelocity = new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * projectileRotation),
                Mathf.Sin(Mathf.Deg2Rad * projectileRotation)
            ) * attackVelocity;
            Vector2 playerVelocity = playerRb.linearVelocity;
            Vector2 finalProjectileVelocity = projectileBaseVelocity + playerVelocity * inheritInertia;

            float finalRotation = Mathf.Rad2Deg * Mathf.Atan2(finalProjectileVelocity.y, finalProjectileVelocity.x);
            projectile.transform.position = new Vector3(transform.position.x, transform.position.y, projectile.transform.position.z);
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, finalRotation);

            projectile.Fire(attackLifeTime, finalProjectileVelocity.magnitude, attackAcceleration, attackGravity, knockback, damage);
            playerRb.AddForce(new Vector2(
                -Mathf.Cos(Mathf.Deg2Rad * weaponRotation),
                -Mathf.Sin(Mathf.Deg2Rad * weaponRotation)
            ) * recoil);
        }
        player.UpdateAmmo(ammoCount);
        barrelAnimator.Play("Recoil", 0, 0);
        if (burstRemaining > 0) {
            barrelAnimator.speed = 50f / Mathf.Max(burstTimeBetweenShots, 0.5f);
        } else {
            barrelAnimator.speed = 50f / Mathf.Max(timeBetweenShots, 0.5f);
        }

        if (ammoCount <= 0 && reloading <= 0) {
            reloading = reloadTime;
        }
    }

    void showMuzzleFlash() {
        flashSprite.sprite = muzzleFlashSprites[Random.Range(0, muzzleFlashSprites.Length)];
        flashSprite.transform.localScale = new Vector3(1, Random.Range(0, 2) * 2 - 1, 1);
        flashSprite.enabled = true;
        flashTimePassed = 0;
    }

    void Update() {
        flashTimePassed += Time.deltaTime;
        if (flashTimePassed > muzzleFlashTime) {
            hideMuzzleFlash();
        }
    }

    void hideMuzzleFlash() {
        flashSprite.enabled = false;
    }

    void OnValidate() {
        UpdateMaxAmmo();
    }

    void UpdateMaxAmmo() {
        if (player) player.UpdateMaxAmmo(maxAmmoCount);
    }
}
