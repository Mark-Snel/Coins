using UnityEngine;
using static ObjectPool<Projectile>;

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
    private Vector3 oldInterpolatedPosition;
    private float oldRotation;

    private float interpolation = 0;
    private float finalDistance = float.MaxValue;
    private bool killOnNextFrame = false;
    private bool ending = false;
    private bool alreadyHit = false;
    private float surfaceNormal;

    private float targetTrailLength;
    private float oldTrailLength;
    private Transform trail;
    private Vector3 defaultTrailScale;

    private Collider2D startCollider;

    private byte fromPlayerId;
    private int projectileId;
    public int ProjectileId {
        get {return projectileId;}
        private set{}
    }
    public static int ProjectileIdCounter = 0;

    void Start() {
        SetTrail();
    }

    public void RegisterHit(Vector2 knockback, int damage, byte fromPlayerId, byte toPlayerId, byte playerId) {
        ending = true;
        finalDistance = 0;
        finalPosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        surfaceNormal = targetRotation - 180;
        if (gameObject.activeInHierarchy && toPlayerId == playerId && !alreadyHit) {
            PlayerController.Instance.Hit(knockback, damage);
            alreadyHit = true;
        }
    }

    public void Fire(byte fromPlayerId, int lifeTime, float velocity, float acceleration, float gravity, float knockback, int damage, int id) {
        SetTrail();
        this.fromPlayerId = fromPlayerId;
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
        ProjectileRegistry.RegisterProjectile(this, fromPlayerId, projectileId);
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

            ImpactEffect impactEffect = ObjectPool<ImpactEffect>.Get();
            if (impactEffect != null) {
                impactEffect.transform.position = finalPosition;
                impactEffect.transform.rotation = Quaternion.Euler(0f, 0f, surfaceNormal - 90);
                impactEffect.Activate(0.03f);
            }
            trail.localScale = new Vector3(
                defaultTrailScale.x,
                Mathf.Min(Mathf.Max(Mathf.Lerp(oldTrailLength, targetTrailLength, interpolationFactor), defaultTrailScale.y), Vector3.Distance(oldInterpolatedPosition, finalPosition)),
                defaultTrailScale.z
            );
            return;
        }

        transform.position = Vector3.Lerp(oldPosition, targetPosition, interpolationFactor);
        if (finalDistance != float.MaxValue) {
            oldInterpolatedPosition = transform.position;
        }
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
            if (firstHit && hit.distance == 0) {
                startCollider = hit.collider;
                stillInside = true;
                firstHit = false;
                continue;
            }

            if (hit.collider != startCollider) {
                Hit(hit);
                return;
            } else {
                stillInside = true;
            }
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
        surfaceNormal = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;
        PlayerController player = hit.collider.GetComponent<PlayerController>();
        if (player != null && !alreadyHit) {
            player.Hit(new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * targetRotation),
                Mathf.Sin(Mathf.Deg2Rad * targetRotation)
            ) * knockback, damage, fromPlayerId, projectileId);
            alreadyHit = true;
        }

        ExternalPlayerController externalPlayer = hit.collider.GetComponent<ExternalPlayerController>();
        if (GameController.playerId == null) Debug.LogWarning("GameController.playerId is NULL");
        if (externalPlayer != null && fromPlayerId == GameController.playerId.Value) {
            externalPlayer.Hit(new Vector2(
                Mathf.Cos(Mathf.Deg2Rad * targetRotation),
                Mathf.Sin(Mathf.Deg2Rad * targetRotation)
            ) * knockback, damage, fromPlayerId, projectileId);
        }
    }

    void Kill() {
        killOnNextFrame = false;
        ending = false;
        interpolation = 0;
        trail.localScale = defaultTrailScale;
        finalDistance = float.MaxValue;
        ProjectileRegistry.UnregisterProjectile(fromPlayerId, projectileId);
        ObjectPool<Projectile>.Return(this);
    }
}
