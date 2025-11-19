using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DamageHandler))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Shield (instant)")]
    public float maxShield = 100f;
    public Image shieldBar;

    [Header("Health (chip effect)")]
    public int maxHealth = 500;               
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;

    [Header("Damage Sources")]
    public LayerMask damageLayers;
    public float contactDamage = 10f;

    [Header("Shield Removal")]
    public GameObject shieldObject;
    public bool removeOnShieldDepleted = true;
    public bool removeOnHealthDepleted = false;
    public bool destroyInsteadOfDisable = false;
    bool _shieldRemoved;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;

    // internal
    float shield;
    float healthLerpTimer;
    bool _playerDead = false;

    DamageHandler _damageHandler;

    void Awake()
    {
        // Ensure DamageHandler exists
        _damageHandler = GetComponent<DamageHandler>();
        if (_damageHandler == null)
            _damageHandler = gameObject.AddComponent<DamageHandler>();

        // Initialize health
        _damageHandler.Init(maxHealth);
    }

    void OnEnable()
    {
        _damageHandler.HealthChanged.AddListener(OnHealthChanged);
        _damageHandler.ObjectDestroyed.AddListener(OnPlayerDestroyed);
    }

    void OnDisable()
    {
        if (_damageHandler != null)
        {
            _damageHandler.HealthChanged.RemoveListener(OnHealthChanged);
            _damageHandler.ObjectDestroyed.RemoveListener(OnPlayerDestroyed);
        }
    }

    void Start()
    {
        shield = maxShield;
        if (shieldBar) shieldBar.fillAmount = 1f;
        if (frontHealthBar) frontHealthBar.fillAmount = 1f;
        if (backHealthBar) backHealthBar.fillAmount = 1f;
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        // clamp shield
        shield = Mathf.Clamp(shield, 0f, maxShield);
        // UI updates handled by events
    }

    // Event called when DamageHandler.HealthChanged is triggered
    void OnHealthChanged()
    {
        int current = _damageHandler.Health;
        int max = _damageHandler.MaxHealth;

        float target = max > 0 ? (float)current / max : 0f;

        if (frontHealthBar && backHealthBar)
            UpdateChipBar(frontHealthBar, backHealthBar, ref healthLerpTimer, target, chipSpeed);

        if (!_playerDead && current <= 0)
            HandleDeath();
    }

    void OnPlayerDestroyed()
    {
        if (!_playerDead)
            HandleDeath();
    }

    void HandleDeath()
    {
        _playerDead = true;

        // Show Game Over UI
        if (gameOverPanel) gameOverPanel.SetActive(true);

        // Notify GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerLost();

        // Detach camera so it isn't destroyed
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform.IsChildOf(transform))
            mainCam.transform.parent = null;

        // Disable all other scripts except this one
        foreach (var comp in GetComponents<MonoBehaviour>())
            if (comp != this) comp.enabled = false;

        // Disable visuals/colliders
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        Debug.Log("Player died -> Game Over. Camera detached.");
    }

    void UpdateChipBar(Image front, Image back, ref float timer, float targetFill, float seconds)
    {
        float f = front.fillAmount;
        float b = back.fillAmount;

        if (b > targetFill)
        {
            front.fillAmount = targetFill;
            back.color = Color.red;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(seconds > 0f ? timer / seconds : 1f);
            t *= t;
            back.fillAmount = Mathf.Lerp(b, targetFill, t);
        }
        else if (f < targetFill)
        {
            back.fillAmount = targetFill;
            back.color = Color.green;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(seconds > 0f ? timer / seconds : 1f);
            t *= t;
            front.fillAmount = Mathf.Lerp(f, targetFill, t);
        }
        else
        {
            timer = 0f;
        }
    }

    public void TakeDamage(float amount)
    {
        if (_playerDead || amount <= 0f) return;

        float remaining = amount;

        // DamageHandler handles integer health
        int damageToHealth = Mathf.FloorToInt(remaining);
        if (damageToHealth > 0)
        {
            _damageHandler.TakeDamage(damageToHealth, transform.position);
            remaining -= damageToHealth;
        }

        // Overflow hits shield instantly
        if (remaining > 0f)
        {
            shield = Mathf.Max(0f, shield - remaining);
            if (shieldBar) shieldBar.fillAmount = maxShield > 0f ? shield / maxShield : 0f;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (((1 << c.gameObject.layer) & damageLayers.value) != 0)
            TakeDamage(contactDamage);
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & damageLayers.value) != 0)
            TakeDamage(contactDamage);
    }

    void TryRemoveShieldIfNeeded()
    {
        if (_shieldRemoved || shieldObject == null) return;

        bool shouldRemove =
            (removeOnShieldDepleted && shield <= 0f) ||
            (removeOnHealthDepleted && _damageHandler != null && _damageHandler.Health <= 0);

        if (shouldRemove)
        {
            _shieldRemoved = true;
            if (destroyInsteadOfDisable) Destroy(shieldObject);
            else shieldObject.SetActive(false);
        }
    }
}
