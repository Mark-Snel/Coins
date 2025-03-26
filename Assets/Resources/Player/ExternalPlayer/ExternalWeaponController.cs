using UnityEngine;
using static ColorManager;
using static ObjectPool<Projectile>;

public class ExternalWeaponController : MonoBehaviour {
    private float interpolationFactor = 0.01f;
    public int reloadTime = 200;
    private int maxAmmoCount = -1;
    public int MaxAmmoCount {
        get { return maxAmmoCount; }
        set {
            if (maxAmmoCount != value) {
                maxAmmoCount = value;
                player.UpdateMaxAmmo(maxAmmoCount);
            }
        }
    }
    public int burstSize = 1;
    public int burstTimeBetweenShots = 5;
    public int timeBetweenShots = 25;
    public float recoil = 0;

    private int ammoCount;
    public int AmmoCount {
        get { return ammoCount; }
        set {
            if (ammoCount != value) {
                ammoCount = value;
                player.UpdateAmmo(ammoCount);
            }
        }
    }
    private int burstRemaining = 0;
    private int burstCooldown = 0;
    private int cooldown = 0;
    private int reloading = 0;

    public int attackCount = 1;

    private Rigidbody2D playerRb;
    private Transform barrel;
    private Animator barrelAnimator;
    public SpriteRenderer baseSprite;
    private SpriteRenderer flashSprite;
    public Sprite[] muzzleFlashSprites;
    private float muzzleFlashTime = 0.02f;//in seconds
    private float flashTimePassed = 0;
    private int color = -2;
    public ExternalPlayerController player;

    private float targetDirection;

    public void SetColor(int color) {
        this.color = color;
        Vector2 hs = GetHueSaturation(color);
        baseSprite.material.SetFloat("_Hue", hs.x);
        baseSprite.material.SetFloat("_Saturation", hs.y);
    }

    void Start() {
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
            }
        } else if (cooldown > 0) cooldown--;
        if (ammoCount <= 0 && reloading <= 0) {
            reloading = reloadTime;
            player.UpdateAmmo(ammoCount);
            player.UpdateReload(reloadTime, reloading);
        } else if (reloading > 0) {
            reloading--;
            player.UpdateReload(reloadTime, reloading);
        }
    }
    public void Attack(byte fromPlayerId, float x, float y, float rotation, int lifeTime, float velocity, float acceleration, float gravity, float knockback, int damage, int projectileId) {
        cooldown = timeBetweenShots;
        burstRemaining--;
        showMuzzleFlash();

        player.UpdateAmmo(ammoCount);
        Projectile projectile = ObjectPool<Projectile>.Get();
        projectile.transform.position = new Vector3(x, y, projectile.transform.position.z);
        projectile.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        projectile.Fire(fromPlayerId, lifeTime, velocity, acceleration, gravity, knockback, damage, projectileId);

        playerRb.AddForce(new Vector2(
            -Mathf.Cos(Mathf.Deg2Rad * rotation),
            -Mathf.Sin(Mathf.Deg2Rad * rotation)
        ) * recoil);
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

    public void Aim(float direction) {
        targetDirection = direction;
    }

    void showMuzzleFlash() {
        flashSprite.sprite = muzzleFlashSprites[Random.Range(0, muzzleFlashSprites.Length)];
        flashSprite.transform.localScale = new Vector3(1, Random.Range(0, 2) * 2 - 1, 1);
        flashSprite.enabled = true;
        flashTimePassed = 0;
    }

    private float angularVelocity = 0f;
    void Update() {
        float direction = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, targetDirection, ref angularVelocity, interpolationFactor);
        if (direction - 90 > 0) {
            transform.localScale = new Vector3(1, -1, 1);
        } else {
            transform.localScale = new Vector3(1, 1, 1);
        }
        transform.rotation = Quaternion.Euler(0, 0, direction);
        flashTimePassed += Time.deltaTime;
        if (flashTimePassed > muzzleFlashTime) {
            hideMuzzleFlash();
        }
    }

    void hideMuzzleFlash() {
        flashSprite.enabled = false;
    }
}
