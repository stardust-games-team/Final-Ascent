using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DamageHandler))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Ship Data")]
    [SerializeField] ShipDataSo shipData;

    [Header("Shield (instant)")]
    public Image shieldBar;

    [Header("Health (chip effect)")]
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

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;

    // internal
    float shield;
    float healthLerpTimer;
    bool _playerDead = false;
    bool _shieldRemoved = false;

    DamageHandler _damageHandler;

    void Awake()
    {
        _damageHandler = GetComponent<DamageHandler>();
        if (_damageHandler == null)
            _damageHandler = gameObject.AddComponent<DamageHandler>();

        // Initialize health via DamageHandler using ShipData
        if (shipData != null)
            _damageHandler.Init(shipData.MaxHealth);
        else
            _damageHandler.Init(500); // fallback if ShipData is missing
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
        // Initialize shield from ShipData
        shield = shipData != null ? shipData.ShieldStrength : 100f;

        if (shieldBar) shieldBar.fillAmount = 1f;
        if (frontHealthBar) frontHealthBar.fillAmount = 1f;
        if (backHealthBar) backHealthBar.fillAmount = 1f;
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    void Update()
    {
        shield = Mathf.Clamp(shield, 0f, shipData != null ? shipData.ShieldStrength : 100f);
    }

    void OnHealthChanged()
    {
        float target = _damageHandler.MaxHealth > 0 ? (float)_damageHandler.Health / _damageHandler.MaxHealth : 0f;

        if (frontHealthBar && backHealthBar)
            UpdateChipBar(frontHealthBar, backHealthBar, ref healthLerpTimer, target, chipSpeed);

        if (!_playerDead && _damageHandler.Health <= 0)
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

        if (gameOverPanel) gameOverPanel.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerLost();

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.transform.IsChildOf(transform))
            mainCam.transform.parent = null;

        foreach (var comp in GetComponents<MonoBehaviour>())
            if (comp != this) comp.enabled = false;

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

    float damageRemaining = amount;

    // Shield absorbs first
    if (shield > 0f)
    {
        float shieldAbsorb = Mathf.Min(shield, damageRemaining);
        shield -= shieldAbsorb;
        damageRemaining -= shieldAbsorb;

        if (shieldBar) shieldBar.fillAmount = shipData != null ? shield / shipData.ShieldStrength : shield / 100f;
    }

    // Remaining damage goes directly to DamageHandler
    if (damageRemaining > 0f)
    {
        int intDamage = Mathf.CeilToInt(damageRemaining); // round up so small damage isn't lost
        _damageHandler.TakeDamage(intDamage, transform.position);
    }

    TryRemoveShieldIfNeeded();
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
            (removeOnHealthDepleted && _damageHandler.Health <= 0);

        if (shouldRemove)
        {
            _shieldRemoved = true;
            if (destroyInsteadOfDisable) Destroy(shieldObject);
            else shieldObject.SetActive(false);
        }
    }
}
