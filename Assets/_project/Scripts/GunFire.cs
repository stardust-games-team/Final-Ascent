using UnityEngine;

public class GunFire : MonoBehaviour
{
    [Header("Spawn")]
    public Transform muzzle;              // optional: assign if you make one
    public float muzzleOffset = 1.2f;     // used if no muzzle Transform

    [Header("Projectile")]
    public GameObject projectilePrefab;   // Sphere w/ Rigidbody + Collider
    public float projectileSpeed = 60f;

    [Header("Input")]
    public KeyCode fireKey = KeyCode.Space;
    public bool autoFire = true;          // hold to fire, or tap if false
    public float fireRate = 8f;           // shots per second

    float nextShotTime;

    void Update()
    {
        bool wantFire = autoFire ? Input.GetKey(fireKey) : Input.GetKeyDown(fireKey);
        if (!wantFire || Time.time < nextShotTime) return;

        nextShotTime = Time.time + 1f / fireRate;
        Fire();
    }

    void Fire()
    {
        if (!projectilePrefab) return;

        // spawn position/rotation based on turret forward (+Z)
        Vector3 fwd = transform.forward;
        Vector3 spawnPos = muzzle ? muzzle.position : (transform.position + fwd * muzzleOffset);
        Quaternion spawnRot = Quaternion.LookRotation(fwd, Vector3.up);

        var go = Instantiate(projectilePrefab, spawnPos, spawnRot);

        // give it velocity if it has a Rigidbody
        if (go.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = fwd * projectileSpeed;
        }

        // avoid colliding with our own colliders
        if (go.TryGetComponent<Collider>(out var projCol))
        {
            foreach (var col in GetComponentsInChildren<Collider>())
                if (col && projCol) Physics.IgnoreCollision(projCol, col, true);
        }
    }
}
