using UnityEngine;
using static InputHandling;

public class WeaponController : MonoBehaviour {
    //weapon behaviour
    [SerializeField] private bool automatic = false;
    [SerializeField] private int reloadTime = 200; //in ticks
    [SerializeField] private int maxAmmoCount = 3;
    [SerializeField] private int burstSize = 1;
    [SerializeField] private int burstTimeBetweenShots = 3; //only applies if burstSize > 1, in ticks
    [SerializeField] private int timeBetweenShots = 25; //in ticks
    private float inheritInertia = 0; //multiplier of how much speed the projectile gets from the velocity of the player when shot.
    private float recoil = 0;

    //projectile behaviour
    private float spread = 0;
    private int attackLifeTime = 200; //in ticks
    private int attackCount = 1; //projectiles per shot, a higher number cause a shotgun-effect
    private float attackVelocity = 1;
    private float attackAcceleration = 0;
    private float attackGravity = 1;
    private float knockback = 1;
    private int damage = 30;

    private int ammoCount;
    private int burstRemaining = 0;
    private int burstCooldown = 0;
    private int cooldown = 0;
    private int reloading = 0;

    void Start() {
        ammoCount = maxAmmoCount;
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
        } else if (reloading > 0) reloading--;
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
        ammoCount--;
        if (ammoCount <= 0 && reloading <= 0) {
            reloading = reloadTime;
        }
    }
}
