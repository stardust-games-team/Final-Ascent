
using UnityEngine;

public class TurretAimToTargetLocal : MonoBehaviour
{
    public Transform target;                 // WorldCrosshair
    public bool smooth = true;
    public float turnSpeed = 720f;           // deg/sec
    public float deadZone = 0.0001f;

    // true = yaw-only; false = pitch+yaw (space)
    public bool flatten = false;
    public bool useParentUp = true;

    public enum AimAxis { Z_Positive, Z_Negative, X_Positive, X_Negative, Y_Positive, Y_Negative }
    public AimAxis aimAxis = AimAxis.Z_Positive; // Unity Cylinder: Y_Positive

    public Transform pivotOverride;          // optional

    Transform Pivot => pivotOverride ? pivotOverride : transform;

    Quaternion AxisToZ(AimAxis a)
    {
        switch (a)
        {
            case AimAxis.Z_Positive: return Quaternion.identity;
            case AimAxis.Z_Negative: return Quaternion.Euler(0, 180, 0);
            case AimAxis.X_Positive: return Quaternion.Euler(0, -90, 0);
            case AimAxis.X_Negative: return Quaternion.Euler(0, 90, 0);
            case AimAxis.Y_Positive: return Quaternion.Euler(90, 0, 0);
            case AimAxis.Y_Negative: return Quaternion.Euler(-90, 0, 0);
        }
        return Quaternion.identity;
    }

    void LateUpdate()
    {
        if (!target) return;

        var p = Pivot;
        var parent = p.parent;

        Vector3 dirW = target.position - p.position;
        if (flatten)
        {
            Vector3 up = useParentUp && parent ? parent.up : Vector3.up;
            dirW -= Vector3.Project(dirW, up); // remove vertical component
        }

        // Convert to parent-local space
        Vector3 dirLocal = parent ? parent.InverseTransformDirection(dirW) : dirW;
        if (dirLocal.sqrMagnitude < deadZone) return;

        Quaternion lookLocal = Quaternion.LookRotation(dirLocal.normalized, Vector3.up);
        Quaternion fix = AxisToZ(aimAxis);
        Quaternion wantLocal = lookLocal * Quaternion.Inverse(fix);

        p.localRotation = smooth
            ? Quaternion.RotateTowards(p.localRotation, wantLocal, turnSpeed * Time.deltaTime)
            : wantLocal;
    }
}
