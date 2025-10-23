using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Shield (instant)")]
    public float maxShield = 100f;
    public Image shieldBar;

    [Header("Health (chip effect)")]
    public float maxHealth = 100f;
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;

    [Header("Damage Sources")]
    public LayerMask damageLayers;
    public float contactDamage = 10f;

    // NEW: which scene object to remove + when
    [Header("Shield Removal")]
    public GameObject shieldObject;                 // assign the Shield prefab instance in scene
    public bool removeOnShieldDepleted = true;      // remove when shield reaches 0
    public bool removeOnHealthDepleted = false;     // or remove when health reaches 0 instead
    public bool destroyInsteadOfDisable = false;    // tick to Destroy(); untick to SetActive(false)
    bool _shieldRemoved;                            // ensures it only happens once

    float shield, health;
    float healthLerpTimer;

    void Start()
    {
        shield = maxShield;
        health = maxHealth;

        if (shieldBar) shieldBar.fillAmount = 1f;
        if (frontHealthBar) frontHealthBar.fillAmount = 1f;
        if (backHealthBar) backHealthBar.fillAmount = 1f;
    }

    void Update()
    {
        shield = Mathf.Clamp(shield, 0f, maxShield);
        health = Mathf.Clamp(health, 0f, maxHealth);

        UpdateUI();
    }

    void UpdateUI()
    {
        if (shieldBar) shieldBar.fillAmount = maxShield > 0f ? shield / maxShield : 0f;

        if (frontHealthBar && backHealthBar)
        {
            float hTarget = maxHealth > 0f ? health / maxHealth : 0f;
            UpdateChipBar(frontHealthBar, backHealthBar, ref healthLerpTimer, hTarget, chipSpeed);
        }

        // NEW: check removal conditions every frame (safe if TakeDamage isn't the only changer)
        TryRemoveShieldIfNeeded();
    }

    void UpdateChipBar(Image front, Image back, ref float timer, float targetFill, float seconds)
    {
        float f = front.fillAmount, b = back.fillAmount;

        if (b > targetFill) { // damage
            front.fillAmount = targetFill;
            back.color = Color.red;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(seconds > 0f ? timer / seconds : 1f); t *= t;
            back.fillAmount = Mathf.Lerp(b, targetFill, t);
        } else if (f < targetFill) { // heal
            back.fillAmount = targetFill; back.color = Color.green;
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(seconds > 0f ? timer / seconds : 1f); t *= t;
            front.fillAmount = Mathf.Lerp(f, targetFill, t);
        } else {
            timer = 0f;
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        float remaining = amount;

        // health first (chip)
        if (health > 0f) {
            float taken = Mathf.Min(health, remaining);
            health -= taken; remaining -= taken;
            healthLerpTimer = 0f;
        }
        // overflow hits shield (instant)
        if (remaining > 0f) {
            shield = Mathf.Max(0f, shield - remaining);
        }

        UpdateUI();
        // TryRemoveShieldIfNeeded(); // also called in UpdateUI; calling here too is fine
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f) return;
        health = Mathf.Min(maxHealth, amount + health);
        healthLerpTimer = 0f;
        UpdateUI();
    }

    public void RestoreShield(float amount)
    {
        if (amount <= 0f) return;
        shield = Mathf.Min(maxShield, amount + shield);
        UpdateUI();
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

    // NEW: one-shot removal
    void TryRemoveShieldIfNeeded()
    {
        if (_shieldRemoved || shieldObject == null) return;

        bool shouldRemove =
            (removeOnShieldDepleted && shield <= 0f) ||
            (removeOnHealthDepleted && health <= 0f);

        if (shouldRemove)
        {
            _shieldRemoved = true;
            if (destroyInsteadOfDisable) Destroy(shieldObject);
            else shieldObject.SetActive(false);
        }
    }

    /* previous shield-first overflow kept (commented) if you ever want to flip back:
    public void TakeDamage(float amount)
    {
        float remaining = amount;
        if (shield > 0f) { var absorbed = Mathf.Min(shield, remaining); shield -= absorbed; remaining -= absorbed; }
        if (remaining > 0f) { health = Mathf.Max(0f, health - remaining); healthLerpTimer = 0f; }
        UpdateUI();
    }
    */
}
