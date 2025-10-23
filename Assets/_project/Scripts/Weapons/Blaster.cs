// using UnityEngine;

// public class Blaster : MonoBehaviour
// {
//     [SerializeField] Transform _muzzle;
//     [SerializeField] Projectile _projectilePrefab;
//     [SerializeField] Transform _crosshair;

//     [SerializeField] bool _smoothAim = true;
//     [SerializeField] float _turnSpeed = 720f;
//     [SerializeField] float _deadZone = 0.0001f;
//     float _coolDownTime = 0.25f;


//     Rigidbody _shipRb;
//     IWeaponControls _weaponInput;


//     bool CanFire
//     {
//         get
//         {
//             _coolDownTime -= Time.deltaTime;
//             return _coolDownTime <= 0f;
//         }
//     }

//     float _coolDown;

//     void Awake()
//     {
//         // Cache once; okay if null (e.g., kinematic ship)
//         _shipRb = GetComponentInParent<Rigidbody>();
//     }

//     void LateUpdate()
//     {
//         if (!_crosshair || !_muzzle) return;

//         Vector3 dirToTarget = _crosshair.position - _muzzle.position;
//         if (dirToTarget.sqrMagnitude < _deadZone) return;

//         Quaternion targetRotation = Quaternion.LookRotation(dirToTarget.normalized);
//         _muzzle.rotation = _smoothAim
//             ? Quaternion.RotateTowards(_muzzle.rotation, targetRotation, _turnSpeed * Time.deltaTime)
//             : targetRotation;
//     }

//     void Update()
//     {
//         if (_weaponInput == null) return;
//         if (CanFire && _weaponInput.PrimaryFired)
//         {
//             FireProjectile();

//         }
//     }

//     public void Init(IWeaponControls weaponInput, float coolDown)
//     {
//         _weaponInput = weaponInput;
//         _coolDown = coolDown;
//     }

//     void FireProjectile()
//     {
//         // Spawn aligned to the muzzle
//         Projectile projectile = Instantiate(_projectilePrefab, _muzzle.position, transform.rotation);
//     }


// }

using UnityEngine;

public class Blaster : MonoBehaviour
{
    [SerializeField] Projectile _projectilePrefab;

    [SerializeField] Transform _muzzle;
    
    float _coolDownTime;
    int _launchForce, _damage;
    private float _duration;
    IWeaponControls _weaponInput;
    
    bool CanFire
    {
        get
        {
            _coolDown -= Time.deltaTime;
            return _coolDown <= 0f;
        }
    }
    
    float _coolDown;
    
    // Update is called once per frame
    void Update()
    {
        if (_weaponInput == null) return;
        if (CanFire && _weaponInput.PrimaryFired)
        {
            FireProjectile();
        } 
    }

    public void Init(IWeaponControls weaponInput, float coolDown, int launchForce, float duration, int damage)
    {
        Debug.Log($"Blaster.Init({weaponInput}, {coolDown}, launchForce, {duration}");
        _weaponInput = weaponInput;
        _coolDownTime = coolDown;
        _launchForce = launchForce;
        _duration = duration;
        _damage = damage;
    }
    
    void FireProjectile()
    {
        _coolDown = _coolDownTime;
        Projectile projectile = Instantiate(_projectilePrefab, _muzzle.position, transform.rotation);
        projectile.gameObject.SetActive(false);
        projectile.Init(_launchForce, _damage, _duration);
        projectile.gameObject.SetActive(true);
    }

}