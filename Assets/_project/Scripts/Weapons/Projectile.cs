using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Detonator _hitEffect;
    float _launchForce;
    int _damage;
    float _range;

    float _duration;
    Rigidbody _rigidBody;

    bool OutOfFuel
    {
        get
        {
            _duration -= Time.deltaTime;
            return _duration <= 0f;
        }
    }
    

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        _rigidBody.AddForce(_launchForce * transform.forward);
        _duration = _range;
    }

    private void OnDisable()
    {
        _rigidBody.linearVelocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        if (OutOfFuel) Destroy(gameObject);
    }

    public void Init(int launchForce, int damage, float range, Vector3 linearVelocity, Vector3 angularVelocity)
    {
        // Debug.Log($"Projectile({launchForce}, {damage}, {range}");
        _launchForce = launchForce;
        _damage = damage;
        _range = range;
        _rigidBody.linearVelocity = linearVelocity;
        _rigidBody.angularVelocity = angularVelocity;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(($"projectile collided with {collision.collider.name}"));
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
        Destroy(gameObject);
    }
}