using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ShipController : MonoBehaviour
{
    [SerializeField] ShipInputControls _inputControls;
    
    [SerializeField] ShipDataSo _shipData;

    Rigidbody _rigidBody;
    [Range(-1f, 1f)]
    float _pitchAmount, _rollAmount, _yawAmount = 0f;

    [SerializeField] List<ShipEngine> _engines;
    [SerializeField] List<Blaster> _blasters;

    IMovementControls MovementInput => _inputControls.MovementControls;
    IWeaponControls WeaponInput => _inputControls.WeaponControls;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
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
             _shipData.BlasterProjectileDuration, _shipData.BlasterDamage);
        }

    }

    void Update()
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
}
