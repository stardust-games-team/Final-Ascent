// using UnityEngine;

// public class Projectile : MonoBehaviour
// {
//     [SerializeField, Range(5000f, 25000f)]  float _muzzleSpeed = 10000f;

//     [SerializeField, Range(10, 1000)] int _damage = 100;

//     [SerializeField, Range(2f, 10f)] float _range = 2f;
    
//     Rigidbody _rb;
//     Collider _shipCollider;
//     bool _launched;

//     void Awake()
//     {
//         _rb = GetComponent<Rigidbody>();
//         // Recommended in Inspector:
//         // - Interpolate = Interpolate
//         // - Collision Detection = Continuous Dynamic
//     }

//     public void SetShipCollider(Collider shipCollider)
//     {
//         _shipCollider = shipCollider;
//     }

//     public void Launch(Vector3 inheritVelocity)
//     {
//         if (_shipCollider)
//             Physics.IgnoreCollision(GetComponent<Collider>(), _shipCollider, true);

//         Vector3 shotDir = transform.forward;
//         _rb.linearVelocity = inheritVelocity + shotDir * _muzzleSpeed;

//         _launched = true;

//         float life = _muzzleSpeed > 0f ? _range / _muzzleSpeed : 0.5f;
//         Destroy(gameObject, life);
//     }

//     // IMPORTANT: no Start fallback. If someone forgets to call Launch,
//     // the projectile just sits there (useful for debugging).

    // void OnCollisionEnter(Collision collision)
    // {
    //     IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
    //     if (damageable != null)
    //     {
    //        Vector3 hitPosition = collision.GetContact(0).point;
    //       damageable.TakeDamage(_damage, hitPosition);
    //     }

    //     Destroy(gameObject); // projectile disappears on impact
    // }

// }

using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] [Range(5000f, 25000f)] float _launchForce = 10000f;
    [SerializeField] [Range(10, 1000)] int _damage = 100;
    [SerializeField] [Range(2f, 10f)] float _range = 2f;
    [SerializeField] private Detonator _hitEffect;


    
    Rigidbody _rigidBody;
    float _duration;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        _rigidBody.AddForce(_launchForce * transform.forward);
        _duration = _range;
    }

    void Update()
    {
    
    }

    void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.collider.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            Vector3 hitPosition = collision.GetContact(0).point;
            damageable.TakeDamage(_damage, hitPosition);
        }
        if (_hitEffect != null)
        {
            Instantiate(_hitEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject); // projectile disappears on impact
    }
}