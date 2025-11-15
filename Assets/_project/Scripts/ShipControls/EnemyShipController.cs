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

    GameObject PlayerShip => GameObject.FindGameObjectWithTag("Player");
    Transform _target;

    GameObject _tempTarget;   // new: keeps track of temporary targets

    public UnityEvent<int> ShipDestroyed = new UnityEvent<int>();

    #region Public data for debugging
    public string ShipState => _state.ToString();
    public string TargetName => _target ? _target.name : "none";

    public string DistanceToTarget
    {
        get
        {
            if (_target)
                return $"{Vector3.Distance(_target.position, _transform.position):F2}";
            return "none";
        }
    }

    public string HealthLevel => $"{_damageHandler.Health}/{_damageHandler.MaxHealth}";
    #endregion

    bool InAttackRange => Vector3.Distance(PlayerShip.transform.position, _transform.position) <= _attackRange;
    bool ShouldRetreat => _damageHandler.Health < (_damageHandler.MaxHealth * 0.33f);

    bool ReachedPatrolTarget =>
        _target &&
        _target != PlayerShip.transform &&
        Vector3.Distance(_target.position, _transform.position) < 0.15f;

    bool ShouldReposition =>
        Physics.SphereCast(_transform.position, 3f, _transform.forward,
            out var hit, 100f, _playerMask);

    public override void OnEnable()
    {
        _transform = transform;
        _aiShipMovementControls = (AIShipMovementControls)_movementControls;
        _aiShipWeaponControls = (AIShipWeaponControls)_weaponControls;

        SetState(EnemyShipState.Patrol);

        base.OnEnable();
    }

    void OnDisable()
    {
        ShipDestroyed.Invoke(GetInstanceID());
    }

    public override void Update()
    {
        EnemyShipState next = GetNextState();
        if (next != _state)
            SetState(next);

        base.Update();
    }

    EnemyShipState GetNextState()
    {
        return _state switch
        {
            EnemyShipState.Patrol => Patrol(),
            EnemyShipState.Attack => Attack(),
            EnemyShipState.Reposition => Reposition(),
            EnemyShipState.Retreat => Retreat(),
            _ => EnemyShipState.None
        };
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

        if (Vector3.Distance(_target.position, _transform.position) < 100f)
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
                CleanupTempTarget(); // <-- prevent destroying the player
                _target = PlayerShip.transform;

                _aiShipMovementControls.SetTarget(_target);
                SetWeaponsTarget(_target, _attackRange, _targetMask);
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
        // If target is null OR target is the player, create new patrol target
        if (_target == null || _target == PlayerShip.transform)
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

        var away = (_transform.position - PlayerShip.transform.position).normalized;
        _tempTarget.transform.position = _transform.position + away * 5000f;

        return _tempTarget.transform;
    }

    // ------------- TEMP TARGET CLEANUP -------------

    void CleanupTempTarget()
    {
        if (_tempTarget)
        {
            Destroy(_tempTarget);
            _tempTarget = null;
        }
    }

    // ------------- WEAPONS HANDLING -------------

    void SetWeaponsTarget(Transform target, float attackRange, int targetMask)
    {
        foreach (var launcher in _missileLaunchers)
            launcher.SetTarget(target);

        _aiShipWeaponControls.SetTarget(target, attackRange, targetMask);
    }
}
