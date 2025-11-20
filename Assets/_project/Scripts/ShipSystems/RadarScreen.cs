using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarScreen : MonoBehaviour
{
    [SerializeField] GameObject _blipPrefab;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] [Range(100, 5000)] float _radarRange = 500f;
    [SerializeField] [Range(0, 1000)] float _lockOnRange = 1000f;
    [SerializeField] [Range(0, 45)] float _lockedOnRadius = 15f;
    [SerializeField] int _maxTargets = 200;
    [SerializeField] float _refreshDelay = 0.25f;
    [SerializeField] GameObject _radarScreen;
    [SerializeField] Transform _player;

    Transform _transform;
    WaitForSeconds _waitForSeconds;
    List<Transform> _targetsInRange;
    float _radarWidth;
    Transform _radarTransform;
    Renderer _radarRenderer;
    Vector3 _targetPosition = Vector3.zero;
    Vector3 _blipPosition = Vector3.zero;
    Vector3 _localDirection;
    float _angleToTarget;
    float _playerAngle;
    float _radarAngle;
    float _normalizedDistance;
    float _angleRadians;
    float _blipX, _blipY;
    float _pitch;
    Collider[] _targetColliders;

    public Transform LockedOnTarget { get; private set; }
    int TargetsInRange => _targetsInRange.Count;
    bool InCombat { get; set; }

    void Awake()
    {
        if (!_radarScreen) return;

        _targetsInRange = new List<Transform>();
        _radarTransform = _radarScreen.transform;
        _radarRenderer = _radarScreen.GetComponent<Renderer>();
        _transform = transform;
        LockedOnTarget = null;
    }

    void OnValidate()
    {
        _targetColliders = new Collider[_maxTargets];
        if (!_radarScreen) return;
        _waitForSeconds = new WaitForSeconds(_refreshDelay);
    }

    void Start()
    {
        if (!_radarRenderer) return;
        _radarWidth = _radarRenderer.bounds.size.x;
    }

    void OnEnable()
    {
        StartCoroutine(nameof(RefreshTargetList));
    }

    void OnDisable()
    {
        StopCoroutine(nameof(RefreshTargetList));
    }

    void LateUpdate()
    {
        DrawTargetBlips();
        UIManager.Instance.UpdateTargetIndicators(
            _targetsInRange,
            LockedOnTarget != null ? LockedOnTarget.GetInstanceID() : -1
        );

        // Combat & game over handling
        if (GameManager.Instance != null && GameManager.Instance.GameState != GameState.GameOver)
        {
            if (TargetsInRange > 0 && !InCombat)
            {
                InCombat = true;
                GameManager.Instance.InCombat(true);
            }
            else if (TargetsInRange == 0 && InCombat)
            {
                InCombat = false;
                GameManager.Instance.InCombat(false);
            }
        }
    }

    IEnumerator RefreshTargetList()
    {
        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.GameState == GameState.GameOver)
                yield break;

            _targetsInRange.Clear();
            LockedOnTarget = null;
            float closest = _lockOnRange;

            int size = Physics.OverlapSphereNonAlloc(_transform.position, _radarRange, _targetColliders, _layerMask);
            for (int i = 0; i < size; ++i)
            {
                if (_targetColliders[i] == null) continue;

                Transform target = GetRootTransform(i);
                if (target == null || !target.gameObject.activeSelf) continue;

                // Lock-on logic
                float distance = Vector3.Distance(target.position, _transform.position);
                float angle = Vector3.Angle(target.position - _transform.position, _transform.forward);

                if (distance < closest && angle < _lockedOnRadius)
                {
                    closest = distance;
                    LockedOnTarget = target;
                }

                if (!_targetsInRange.Contains(target))
                    _targetsInRange.Add(target);
            }

            yield return _waitForSeconds;
        }
    }

    void DrawTargetBlips()
{
    if (_radarTransform == null || _blipPrefab == null) return;

    ClearDisplay();

    foreach (var target in _targetsInRange)
    {
        if (target == null) continue;

        _targetPosition = (target.position - _player.position) / _radarRange;
        CalculateBlipPosition();

        // null check added here
        if (_blipPrefab != null)
        {
            var blip = Instantiate(_blipPrefab, _radarTransform);
            blip.transform.localScale = _blipPrefab.transform.localScale;
            blip.transform.localPosition = _blipPosition;
            DrawHeightLines(blip);
        }
    }
}


    void DrawHeightLines(GameObject blip)
    {
        if (Mathf.Approximately(0f, _pitch)) return;

        RadarBlip radarBlip = blip.GetComponent<RadarBlip>();
        radarBlip.LineAbove.SetActive(_pitch < 0f);
        radarBlip.LineBelow.SetActive(_pitch > 0f);

        Vector3 scale;
        if (_pitch < 0f)
        {
            scale = radarBlip.LineAbove.transform.localScale;
            scale.y = Mathf.Clamp(Mathf.Abs(_pitch) / 90f, 0f, 1f);
            radarBlip.LineAbove.transform.localScale = scale;
        }
        else if (_pitch > 0f)
        {
            scale = radarBlip.LineBelow.transform.localScale;
            scale.y = Mathf.Clamp(Mathf.Abs(_pitch) / 90f, 0f, 1f);
            radarBlip.LineBelow.transform.localScale = scale;
        }
    }

    void CalculateBlipPosition()
    {
        _angleToTarget = Mathf.Atan2(_targetPosition.x, _targetPosition.z) * Mathf.Rad2Deg;
        _localDirection = Quaternion.Inverse(_player.rotation) * _targetPosition;
        _pitch = Vector3.Angle(Vector3.down, _localDirection) - 90f;
        _playerAngle = _player.eulerAngles.y;
        _radarAngle = _angleToTarget - _playerAngle - 90f;
        _normalizedDistance = _targetPosition.magnitude;
        _angleRadians = _radarAngle * Mathf.Deg2Rad;
        _blipX = _normalizedDistance * Mathf.Cos(_angleRadians);
        _blipY = _normalizedDistance * Mathf.Sin(_angleRadians);
        _blipPosition.x = _blipX * (_radarWidth * 0.25f);
        _blipPosition.y = _blipY * (_radarWidth * 0.25f) * -1f;

        if (_player.localEulerAngles.z < 270f && _player.localEulerAngles.z > 90f)
            _blipPosition.x *= -1f;

        _blipPosition.z = -0.01f;
    }

    void ClearDisplay()
    {
        if (_radarTransform == null) return;

        for (int i = _radarTransform.childCount - 1; i >= 0; i--)
        {
            var child = _radarTransform.GetChild(i);
            if (child != null)
            {
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }
    }

    Transform GetRootTransform(int i)
    {
        if (_targetColliders[i] == null) return null;

        Transform root = _targetColliders[i].transform;
        int layer = root.gameObject.layer;

        while (root.parent != null && layer == root.parent.gameObject.layer)
        {
            root = root.parent;
        }

        return root;
    }
}
