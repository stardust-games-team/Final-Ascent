using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyShipController : ShipController
{
    [SerializeField] float _patrolRange = 2000f, _attackRange = 1000f;
    [SerializeField] LayerMask _targetMask, _playerMask;

    enum EnemyShipState
    {
        None,
        Patrol,
        Attack,
        Reposition,
        Retreat
    };

    AIShipMovementControls _aiShipMovementControls;
    AIShipWeaponControls _aiShipWeaponControls;
    EnemyShipState _state = EnemyShipState.None;
    Transform _transform;

    // Cache the player reference instead of searching each time
    GameObject _playerShip;
    GameObject PlayerShip
    {
        get
        {
            // If cached player is null or destroyed, try to find it again
            if (_playerShip == null)
            {
                _playerShip = GameObject.FindGameObjectWithTag("Player");
            }
            return _playerShip;
        }
    }

    Transform _target;
    GameObject _tempTarget;
    int _instanceID; // Cache the instance ID

    public UnityEvent<int> ShipDestroyed = new UnityEvent<int>();
    bool _destroyed;

    #region Public data for debugging
    public string ShipState => _state.ToString();
    
    public string TargetName
    {
        get
        {
            // Check if target exists AND hasn't been destroyed
            if (_target != null && _target.gameObject != null)
                return _target.name;
            return "none";
        }
    }

    public string DistanceToTarget
    {
        get
        {
            // Check if both objects exist before calculating distance
            if (_target != null && _target.gameObject != null && _transform != null)
            {
                try
                {
                    return $"{Vector3.Distance(_target.position, _transform.position):F2}";
                }
                catch
                {
                    return "none";
                }
            }
            return "none";
        }
    }

    public string HealthLevel
    {
        get
        {
            if (_damageHandler != null)
                return $"{_damageHandler.Health}/{_damageHandler.MaxHealth}";
            return "none";
        }
    }
    #endregion

    // Added null checks for PlayerShip
    bool InAttackRange
    {
        get
        {
            if (PlayerShip == null || PlayerShip.transform == null)
                return false;
            return Vector3.Distance(PlayerShip.transform.position, _transform.position) <= _attackRange;
        }
    }

    bool ShouldRetreat => _damageHandler.Health < (_damageHandler.MaxHealth * 0.33f);

    bool ReachedPatrolTarget =>
        _target &&
        _target != (PlayerShip ? PlayerShip.transform : null) &&
        Vector3.Distance(_target.position, _transform.position) < 0.15f;

    bool ShouldReposition =>
        Physics.SphereCast(_transform.position, 3f, _transform.forward,
            out var hit, 100f, _playerMask);

    public override void OnEnable()
    {
        _transform = transform;
        _instanceID = gameObject.GetInstanceID(); // Cache instance ID early
        _destroyed = false; // Reset destroyed flag
        _aiShipMovementControls = (AIShipMovementControls)_movementControls;
        _aiShipWeaponControls = (AIShipWeaponControls)_weaponControls;

        SetState(EnemyShipState.Patrol);

        // Call base FIRST - this adds:
        // - _damageHandler.HealthChanged.AddListener(OnHealthChanged)
        // - _damageHandler.ObjectDestroyed.AddListener(DestroyShip)
        base.OnEnable();

        // Now we need OnShipDied to run BEFORE DestroyShip
        // Since we can't reorder existing listeners, we need a workaround:
        // Subscribe our own callback that sets _destroyed before anything else
        if (_damageHandler != null)
        {
            // Subscribe to HealthChanged to intercept before health reaches 0
            _damageHandler.HealthChanged.AddListener(CheckIfDying);
            
            Debug.Log($"Enemy {_instanceID} subscribed to damage events");
        }
        else
        {
            Debug.LogError("DamageHandler is null in EnemyShipController!");
        }
    }

   void OnDisable()
{
    // Skip if the game is over
    if (GameManager.Instance != null && GameManager.Instance.GameState == GameState.GameOver)
        return;

    // Only fire ShipDestroyed if truly destroyed (health <= 0)
    if (_destroyed)
    {
        ShipDestroyed.Invoke(_instanceID);
    }

    // Reset temporary targets
    CleanupTempTarget();
}


    void OnDestroy()
    {
        // Unsubscribe from damage handler
        if (_damageHandler != null)
        {
            _damageHandler.HealthChanged.RemoveListener(CheckIfDying);
        }
        
        // Clean up temp targets when this object is destroyed
        CleanupTempTarget();
    }

    // Called when health changes - check if we're about to die
    void CheckIfDying()
    {
        if (_damageHandler.Health <= 0 && !_destroyed)
        {
            Debug.Log($"CheckIfDying: Enemy {_instanceID} health reached 0 - setting _destroyed = true");
            _destroyed = true;
            CleanupTempTarget();
        }
    }

    public override void Update()
    {
        if (_destroyed) return;
        
        EnemyShipState next = GetNextState();
        if (next != _state)
            SetState(next);

        base.Update();
    }

    EnemyShipState GetNextState()
    {
        if (_destroyed) return EnemyShipState.None;
        
        EnemyShipState newState = _state switch
        {
            EnemyShipState.Patrol => Patrol(),
            EnemyShipState.Attack => Attack(),
            EnemyShipState.Reposition => Reposition(),
            EnemyShipState.Retreat => Retreat(),
            _ => EnemyShipState.None
        };
        return newState;
    }

    // ----------------- STATE LOGIC -----------------

    EnemyShipState Patrol()
    {
        if (ShouldRetreat) return EnemyShipState.Retreat;
        if (InAttackRange) return EnemyShipState.Attack;

        if (ReachedPatrolTarget)
        {
            _target.position = Random.insideUnitSphere * _patrolRange;
        }

        return EnemyShipState.Patrol;
    }

    EnemyShipState Attack()
    {
        if (ShouldRetreat) return EnemyShipState.Retreat;
        return ShouldReposition ? EnemyShipState.Reposition : EnemyShipState.Attack;
    }

    EnemyShipState Reposition()
    {
        if (ShouldRetreat) return EnemyShipState.Retreat;

        if (_target && Vector3.Distance(_target.position, _transform.position) < 100f)
            return EnemyShipState.Attack;

        return EnemyShipState.Reposition;
    }

    EnemyShipState Retreat()
    {
        return EnemyShipState.Retreat;
    }

    // ----------------- STATE SWITCHING -----------------

    void SetState(EnemyShipState state)
    {
        if (_state == state) return;
        _state = state;

        switch (state)
        {
            case EnemyShipState.Patrol:
                CreateOrReusePatrolTarget();
                _aiShipMovementControls.SetTarget(_target);
                break;

            case EnemyShipState.Attack:
                CleanupTempTarget();
                
                // Check if player exists before setting as target
                if (PlayerShip != null)
                {
                    _target = PlayerShip.transform;
                    _aiShipMovementControls.SetTarget(_target);
                    SetWeaponsTarget(_target, _attackRange, _targetMask);
                }
                else
                {
                    // Fallback to patrol if player is gone
                    SetState(EnemyShipState.Patrol);
                }
                break;

            case EnemyShipState.Reposition:
                CleanupTempTarget();
                _target = CreateRepositionTarget();

                _aiShipMovementControls.SetTarget(_target);
                SetWeaponsTarget(null, 0, 0);
                break;

            case EnemyShipState.Retreat:
                CleanupTempTarget();
                _target = CreateRetreatTarget();

                _aiShipMovementControls.SetTarget(_target);
                SetWeaponsTarget(null, 0, 0);
                break;
        }
    }

    // ------------- TARGET CREATION METHODS -------------

    void CreateOrReusePatrolTarget()
    {
        // Check if player exists before comparing
        Transform playerTransform = PlayerShip ? PlayerShip.transform : null;
        
        // If target is null OR target is the player, create new patrol target
        if (_target == null || _target == playerTransform)
        {
            CleanupTempTarget();

            _tempTarget = new GameObject("Patrol Target");
            _tempTarget.transform.position = Random.insideUnitSphere * _patrolRange;
            _target = _tempTarget.transform;
        }
    }

    Transform CreateRepositionTarget()
    {
        _tempTarget = new GameObject("Reposition Target");

        var randDir = Random.Range(1, 5) switch
        {
            1 => _transform.right,
            2 => -_transform.right,
            3 => _transform.up,
            4 => -_transform.up,
            _ => -_transform.forward
        };

        _tempTarget.transform.position = _transform.position + randDir * 250f;
        return _tempTarget.transform;
    }

    Transform CreateRetreatTarget()
    {
        _tempTarget = new GameObject("Retreat Target");

        // Check if player exists before calculating retreat direction
        if (PlayerShip != null)
        {
            var away = (_transform.position - PlayerShip.transform.position).normalized;
            _tempTarget.transform.position = _transform.position + away * 5000f;
        }
        else
        {
            // If no player, just retreat in current forward direction
            _tempTarget.transform.position = _transform.position + _transform.forward * 5000f;
        }

        return _tempTarget.transform;
    }

    // ------------- TEMP TARGET CLEANUP -------------

    void CleanupTempTarget()
    {
        if (_tempTarget != null)
        {
            Destroy(_tempTarget);
            _tempTarget = null;
        }
    }

    // ------------- WEAPONS HANDLING -------------

    void SetWeaponsTarget(Transform target, float attackRange, int targetMask)
    {
        if (_missileLaunchers != null)
        {
            foreach (var launcher in _missileLaunchers)
            {
                if (launcher != null)
                    launcher.SetTarget(target);
            }
        }

        if (_aiShipWeaponControls != null)
            _aiShipWeaponControls.SetTarget(target, attackRange, targetMask);
    }
}