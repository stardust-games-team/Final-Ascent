// ShieldImpactRelay.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShieldImpactRelay : MonoBehaviour
{
    [SerializeField] LayerMask damageLayers;   // same layers you want to hurt the player
    [SerializeField] float contactDamage = 10f;

    PlayerHealth _playerHealth;

    void Awake()
    {
        // Find PlayerHealth up the hierarchy (Player root or Ship)
        _playerHealth = GetComponentInParent<PlayerHealth>();
        // if (_playerHealth == null)
        //     Debug.LogWarning("ShieldImpactRelay: No PlayerHealth found in parents.");
    }

    void OnCollisionEnter(Collision c)
    {
        if (_playerHealth == null) return;
        if (((1 << c.gameObject.layer) & damageLayers.value) == 0) return;

        _playerHealth.TakeDamage(contactDamage);   // shield absorbs first, overflow to health
    }

    void OnTriggerEnter(Collider other)
    {
        if (_playerHealth == null) return;
        if (((1 << other.gameObject.layer) & damageLayers.value) == 0) return;

        _playerHealth.TakeDamage(contactDamage);
    }
}
