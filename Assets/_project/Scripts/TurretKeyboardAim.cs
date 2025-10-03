using UnityEngine;

public class TurretKeyboardAim : MonoBehaviour
{
    [Header("Target to face (drag your WorldCrosshair here)")]
    public Transform target;

    [Header("Rotation")]
    public bool smoothRotation = true;
    public float turnSpeed = 720f;   // deg/sec when smoothing

    void Update()
    {
        if (!target) return;

        Vector3 a = transform.position;
        Vector3 b = target.position;

        // flatten on XZ so we only rotate around Y
        Vector3 dir = new Vector3(b.x - a.x, 0f, b.z - a.z);
        if (dir.sqrMagnitude < 0.0001f) return;

        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        Quaternion want = Quaternion.Euler(0f, yaw, 0f);

        transform.rotation = smoothRotation
            ? Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime)
            : want;
    }
}
