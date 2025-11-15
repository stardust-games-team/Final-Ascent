using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour
{
    [SerializeField] Shield _shield;
    [SerializeField]  protected MovementControlsBase _movementControls;

    [SerializeField] protected WeaponControlsBase _weaponControls;

    
    [SerializeField] ShipDataSo _shipData;

    Rigidbody _rigidBody;
    [Range(-1f, 1f)]
    float _pitchAmount, _rollAmount, _yawAmount = 0f;
    protected DamageHandler _damageHandler;

    [SerializeField] List<ShipEngine> _engines;
    [SerializeField] List<Blaster> _blasters;

    [SerializeField] protected List<MissileLauncher> _missileLaunchers;

    IMovementControls MovementInput => _movementControls;
    IWeaponControls WeaponInput =>  _weaponControls;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _damageHandler = GetComponent<DamageHandler>();
    }

    void Start()
    {
        foreach (ShipEngine engine in _engines)
        {
            engine.Init(MovementInput, _rigidBody, _shipData.ThrustForce / _engines.Count);
        }

        foreach (Blaster blaster in _blasters)
        {
            blaster.Init(WeaponInput, _shipData.BlasterCooldown, _shipData.BlasterLaunchForce,
             _shipData.BlasterProjectileDuration, _shipData.BlasterDamage, _rigidBody);
        }

        foreach (MissileLauncher launcher in _missileLaunchers)
        {
            launcher.Init(WeaponInput);
        }

        if (_shield)
        {
            _shield.Init(_shipData.ShieldStrength);
        }

    }

    public virtual void OnEnable()
    {
        if (_damageHandler == null) return;
        _damageHandler.Init(_shipData.MaxHealth);
        _damageHandler.HealthChanged.AddListener(OnHealthChanged);
        _damageHandler.ObjectDestroyed.AddListener(DestroyShip);
    }



    public virtual void Update()
    {
        _rollAmount = MovementInput.RollAmount;
        _yawAmount = MovementInput.YawAmount;
        _pitchAmount = MovementInput.PitchAmount;
    }

    void FixedUpdate()
    {
        if (!Mathf.Approximately(a: 0f, b: _pitchAmount))
        {
            _rigidBody.AddTorque(transform.right * (_shipData.PitchForce * _pitchAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(a: 0f, b: _rollAmount))
        {
            _rigidBody.AddTorque(transform.forward * (_shipData.RollForce * _rollAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(a: 0f, b: _yawAmount))
        {
            _rigidBody.AddTorque(transform.up * (_shipData.YawForce * _yawAmount * Time.fixedDeltaTime));
        }
    }
    
    void DestroyShip()
    {
        gameObject.SetActive(false);
    }

    void OnHealthChanged()
    {
        Debug.Log($"{gameObject.name} health is {_damageHandler.Health}/{_damageHandler.MaxHealth}");
    }
}
