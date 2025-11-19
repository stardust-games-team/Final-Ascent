using UnityEngine;
using UnityEngine.Events;

public class DamageHandler : MonoBehaviour, IDamageable
{
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] float _destroyDelay = 0.5f;

    UnityEvent _healthChangedEvent;
    UnityEvent _objectDestroyedEvent;

    public int MaxHealth { get; private set; }
    public int Health { get; private set; }

    bool _isDestroyed = false;

    public UnityEvent HealthChanged => _healthChangedEvent ??= new UnityEvent();
    public UnityEvent ObjectDestroyed => _objectDestroyedEvent ??= new UnityEvent();

    public void Init(int maxHealth)
    {
        Health = MaxHealth = maxHealth;
        _isDestroyed = false;
        HealthChanged.Invoke();
    }

    public void TakeDamage(int damage, Vector3 hitPosition)
    {
        if (_isDestroyed) return;

        Health -= damage;
        HealthChanged.Invoke();

        Debug.Log($"TakeDamage: {damage} damage taken. Health: {Health}/{MaxHealth}");

        if (Health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;

        Debug.Log($"{gameObject.name} died! Invoking ObjectDestroyed event.");

        if (_explosionPrefab != null)
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        ObjectDestroyed.Invoke();

        // **Instead of destroying player, disable components (handled in PlayerHealth)**
    }
}
