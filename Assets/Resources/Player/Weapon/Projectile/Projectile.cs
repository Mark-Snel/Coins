using UnityEngine;
using static ProjectilePool;

public class Projectile : MonoBehaviour {
    private int lifeTime;
    private float velocity;
    private float acceleration;
    private float gravity;
    private float knockback;
    private int damage;

    private Vector3 targetPosition;
    private Vector3 finalPosition;
    private float targetRotation;
    private Vector3 oldPosition;
    private float oldRotation;

    private float interpolation = 0;
    private float finalDistance = float.MaxValue;
    private bool killOnNextFrame = false;
    private bool ending = false;

    private float targetTrailLength;
    private float oldTrailLength;
    private Transform trail;
    private Vector3 defaultTrailScale;

    private Collider2D startCollider;

    void Start() {
        SetTrail();
    }

    public void Fire(int lifeTime, float velocity, float acceleration, float gravity, float knockback, int damage) {
        SetTrail();
        this.lifeTime = lifeTime;
        this.velocity = velocity/50f;
        this.acceleration = acceleration/50f;
        this.gravity = gravity/50f;
        this.knockback = knockback;
        this.damage = damage;
        targetPosition = transform.position;
        oldPosition = targetPosition;
        targetRotation = transform.rotation.eulerAngles.z;
        oldRotation = targetRotation;
        targetTrailLength = defaultTrailScale.y;
        oldTrailLength = targetTrailLength;
        Tick(true);
        Update();
    }

    void SetTrail() {
        if (trail == null) {
            trail = transform.Find("Trail");
            defaultTrailScale = trail.localScale;
        }
    }

    void Update() {
        if (killOnNextFrame) Kill();
        interpolation += Time.deltaTime;
        float interpolationFactor = interpolation / 0.02f;
        if (finalDistance < interpolationFactor) {
            transform.position = finalPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, targetRotation);
            trail.localScale = new Vector3(defaultTrailScale.x, Mathf.Max(targetTrailLength, defaultTrailScale.y), defaultTrailScale.z);
            killOnNextFrame = true;
            return;
        }
        transform.position = Vector3.Lerp(oldPosition, targetPosition, interpolationFactor);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(oldRotation, targetRotation, interpolationFactor));
        trail.localScale = new Vector3(defaultTrailScale.x, Mathf.Max(Mathf.Lerp(oldTrailLength, targetTrailLength, interpolationFactor), defaultTrailScale.y), defaultTrailScale.z);
    }

    void FixedUpdate() {
        interpolation = 0;
        if (!ending) Tick();
    }

    void Tick(bool firstHit = false) {
        Vector2 velocityVector = new Vector2(
            Mathf.Cos(Mathf.Deg2Rad * targetRotation),
            Mathf.Sin(Mathf.Deg2Rad * targetRotation)
        ) * (velocity + acceleration);

        if (!firstHit) velocityVector.y -= gravity;

        oldPosition = targetPosition;
        oldRotation = targetRotation;
        targetPosition = new Vector3(targetPosition.x + velocityVector.x, targetPosition.y + velocityVector.y, transform.position.z);
        targetRotation = Mathf.Rad2Deg * Mathf.Atan2(velocityVector.y, velocityVector.x);
        velocity = velocityVector.magnitude;
        oldTrailLength = targetTrailLength;
        targetTrailLength = velocity;

        RaycastHit2D[] hits = Physics2D.RaycastAll(oldPosition, velocityVector.normalized, velocity);

        bool stillInside = false;
        foreach (RaycastHit2D hit in hits) {
            if (firstHit) {
                if (hit.distance == 0) {
                    startCollider = hit.collider;
                    stillInside = true;
                     continue;
                }
            }
            if (startCollider == hit.collider) {
                stillInside = true;
                continue;
            }

            Hit(hit);
        }
        if (!stillInside) {
            startCollider = null;
        }

        lifeTime--;
        if (lifeTime <= 0) {
            Kill();
        }
    }

    void Hit(RaycastHit2D hit) {
        ending = true;
        finalDistance = hit.distance/velocity;
        finalPosition = new Vector3(hit.point.x, hit.point.y, transform.position.z);
        PlayerController player = hit.collider.GetComponent<PlayerController>();
        if (player != null) {
            player.Hit(new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * targetRotation),
                Mathf.Sin(Mathf.Deg2Rad * targetRotation)
            ) * knockback, damage);
        }
    }

    void Kill() {
        killOnNextFrame = false;
        ending = false;
        interpolation = 0;
        trail.localScale = defaultTrailScale;
        finalDistance = float.MaxValue;
        ProjectilePool.ReturnProjectile(this);
    }
}
