using UnityEngine;

public class ShipController : MonoBehaviour
{
    // Pitch is up and down, Yaw is left right, Roll is rotation, and thrust is forward or backward
    [SerializeField]
    [Range(1000f, 10000f)]
    float _thrustForce = 5000f,
     _pitchForce = 2000f,
      _rollForce = 1000f,
       _yawForce = 1500f;

    Rigidbody _rigidBody;
    [Range(-1f, 1f)]
    float _thrustAmount, _pitchAmount, _rollAmount, _yawAmount = 0f;

    IMovementControls _movementInput;
    IMovementControls ControlInput => _movementInput;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();

        _movementInput = new DesktopMovementControls();
    }

    void Update()
    {
        _thrustAmount = ControlInput.ThrustAmount;
        _rollAmount = ControlInput.RollAmount;
        _yawAmount = ControlInput.YawAmount;
        _pitchAmount = ControlInput.PitchAmount;
    }

    void FixedUpdate()
    {
        if (!Mathf.Approximately(a: 0f, b: _pitchAmount))
        {
            _rigidBody.AddTorque(transform.right * (_pitchForce * _pitchAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(a: 0f, b: _rollAmount))
        {
            _rigidBody.AddTorque(transform.forward * (_rollForce * _rollAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(a: 0f, b: _yawAmount))
        {
            _rigidBody.AddTorque(transform.up * (_yawForce * _yawAmount * Time.fixedDeltaTime));
        }

        if (!Mathf.Approximately(a: 0f, b: _thrustAmount))
        {
            _rigidBody.AddForce(transform.forward * (_thrustForce * _thrustAmount * Time.fixedDeltaTime));
        }

        
    }
}
