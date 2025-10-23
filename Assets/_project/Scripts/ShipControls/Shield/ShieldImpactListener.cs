using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShieldImpactListener : MonoBehaviour
{
    [SerializeField] LayerMask damageLayers;     // set to Asteroid layer(s)
    [SerializeField] int damage = 1;
    [SerializeField] bool reactToTriggers = true;

    IDamageable _damageable;

    void Awake()
    {
        // Find the IDamageable on this object or a parent
        _damageable = GetComponentInParent<IDamageable>();
        if (_damageable == null)
            Debug.LogWarning("ShieldImpactListener: No IDamageable found on this object or its parents.");
    }

    void OnCollisionEnter(Collision c)
    {
        if (_damageable == null) return;
        if ((damageLayers.value & (1 << c.gameObject.layer)) == 0) return;

        var hit = c.contactCount > 0 ? c.GetContact(0).point : transform.position;
        _damageable.TakeDamage(damage, hit);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!reactToTriggers || _damageable == null) return;
        if ((damageLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var hit = other.ClosestPoint(transform.position);
        _damageable.TakeDamage(damage, hit);
    }
}
