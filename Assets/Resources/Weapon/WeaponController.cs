using UnityEngine;

public class WeaponController : MonoBehaviour {
    //weapon behaviour
    private bool Automatic = false;
    private int reloadTime = 150; //in ticks
    private int maxAmmoCount = 3;
    private int burstSize = 1;
    private int burstSpeed = 3; //only applies if burstSize > 1, in ticks
    private int timeBetweenShots = 15; //in ticks
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
    private int reloading;

    void Start() {
        
    }

    void Update() {
        
    }
}
