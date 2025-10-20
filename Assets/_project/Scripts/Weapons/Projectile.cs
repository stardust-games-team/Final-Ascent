using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField, Range(10f, 400f)]
    float _muzzleSpeed = 200f;

    [SerializeField, Range(2f, 10f)]
    float _range = 5f;

    Rigidbody _rb;
    Collider _shipCollider;
    bool _launched;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Recommended in Inspector:
        // - Interpolate = Interpolate
        // - Collision Detection = Continuous Dynamic
    }

    public void SetShipCollider(Collider shipCollider)
    {
        _shipCollider = shipCollider;
    }

    public void Launch(Vector3 inheritVelocity)
    {
        if (_shipCollider)
            Physics.IgnoreCollision(GetComponent<Collider>(), _shipCollider, true);

        Vector3 shotDir = transform.forward;
        _rb.linearVelocity = inheritVelocity + shotDir * _muzzleSpeed;

        _launched = true;

        float life = _muzzleSpeed > 0f ? _range / _muzzleSpeed : 0.5f;
        Destroy(gameObject, life);
    }

    // IMPORTANT: no Start fallback. If someone forgets to call Launch,
    // the projectile just sits there (useful for debugging).
}
