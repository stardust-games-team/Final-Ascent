using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField] Transform _muzzle;
    [SerializeField] Projectile _projectilePrefab;
    [SerializeField] Transform _crosshair;

    [SerializeField] bool _smoothAim = true;
    [SerializeField] float _turnSpeed = 720f;
    [SerializeField] float _deadZone = 0.0001f;

    Rigidbody _shipRb;

    void Awake()
    {
        // Cache once; okay if null (e.g., kinematic ship)
        _shipRb = GetComponentInParent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (!_crosshair || !_muzzle) return;

        Vector3 dirToTarget = _crosshair.position - _muzzle.position;
        if (dirToTarget.sqrMagnitude < _deadZone) return;

        Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized);
        _muzzle.rotation = _smoothAim
            ? Quaternion.RotateTowards(_muzzle.rotation, targetRotation, _turnSpeed * Time.deltaTime)
            : targetRotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireProjectile();

        }
    }

    void FireProjectile()
    {
        // Spawn aligned to the muzzle
        Projectile projectile = Instantiate(_projectilePrefab, _muzzle.position, transform.rotation);

        // Inherit the ship's current world velocity if available
        // Vector3 inheritVel = _shipRb ? _shipRb.linearVelocity : Vector3.zero;

        // Give the projectile the ship collider to ignore
        // ShipController ship = GetComponentInParent<ShipController>();
        // if (ship) projectile.SetShipCollider(ship.GetComponent<Collider>());

        // Tell the projectile to launch with inherited velocity
        // projectile.Launch(inheritVel);
    }
}
