using UnityEngine;

[CreateAssetMenu(fileName = "ShipData", menuName = "3D Space Shooter/Ship Data", order = 1)]
public class ShipDataSo : ScriptableObject
{
    [SerializeField] [Range(1000f, 50000f)]
    float _thrustForce = 7500f;
    [SerializeField] [Range(1000f, 10000f)]
    float _pitchForce = 6000f;
    [SerializeField] [Range(1000f, 10000f)]
    float _rollForce = 1000f;
    [SerializeField] [Range(1000f, 10000f)]
    float _yawForce = 2000f;

    [SerializeField]
    int _shieldStrength = 5000;
    [SerializeField]
    int _shieldRegenAmount = 100;
    [SerializeField]
    int _maxHealth = 5000;

    [SerializeField] [Range(10, 1000)]
    int _blasterDamage = 100;
    
    [SerializeField] [Range(5000f, 25000f)]
    int _blasterLaunchForce = 10000;
    
    [SerializeField]
    float _blasterCoolDown = 0.25f;
    
    [SerializeField] [Range(2f, 10f)]
    float _blasterProjectileDuration = 2f;

    public float ThrustForce => _thrustForce;
    public float PitchForce => _pitchForce;
    public float RollForce => _rollForce;
    public float YawForce => _yawForce;
    public int ShieldStrength => _shieldStrength;
    public int ShieldRegenAmount => _shieldRegenAmount;
    public int MaxHealth => _maxHealth;
    public int BlasterLaunchForce => _blasterLaunchForce;
    public int BlasterDamage => _blasterDamage;
    public float BlasterProjectileDuration => _blasterProjectileDuration;
    public float BlasterCooldown => _blasterCoolDown;
}
