
using UnityEngine;

public class CrosshairOnShipPlane : MonoBehaviour
{
    [Header("Refs")]
    public Transform ship;          // Ship root (required)
    public Camera cam;              // optional; for billboarding only

    [Header("Placement (ship-local)")]
    public float forwardDistance = 10f; // along ship.local Z
    public float maxOffsetX = 4f;       // ship.local X
    public float maxOffsetY = 3f;       // ship.local Y
    public bool parentedToShip = false; // set TRUE if WorldCrosshair is a child of ship

    [Header("Movement")]
    public float moveSpeed = 8f;        // offset units/sec
    public bool recenterWhenNoInput = false;
    public float recenterSpeed = 3f;

    [Header("Smoothing")]
    public bool smooth = true;
    public float followLerp = 15f;      // higher = snappier

    private Vector2 offset;

    void Awake(){ if (!cam) cam = Camera.main; }

    void LateUpdate()
    {
        if (!ship) return;

        // --- Input in ship-local plane (no camera dependency) ---
        float x = (Input.GetKey(KeyCode.D)?1f:0f) - (Input.GetKey(KeyCode.A)?1f:0f);
        float y = (Input.GetKey(KeyCode.W)?1f:0f) - (Input.GetKey(KeyCode.S)?1f:0f);
        Vector2 dir = new Vector2(x, y);

        if (dir.sqrMagnitude > 0f)
        {
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            offset += dir * moveSpeed * Time.deltaTime;
        }
        else if (recenterWhenNoInput)
        {
            offset = Vector2.Lerp(offset, Vector2.zero, 1f - Mathf.Exp(-recenterSpeed * Time.deltaTime));
            if (offset.sqrMagnitude < 1e-4f) offset = Vector2.zero;
        }

        // Clamp
        offset.x = Mathf.Clamp(offset.x, -maxOffsetX, maxOffsetX);
        offset.y = Mathf.Clamp(offset.y, -maxOffsetY, maxOffsetY);

        // Desired position expressed in SHIP LOCAL space
        Vector3 localPos = new Vector3(offset.x, offset.y, forwardDistance);

        // Apply as local or world (both paths run in LateUpdate)
        if (parentedToShip && transform.parent == ship)
        {
            Vector3 cur = transform.localPosition;
            Vector3 next = smooth
                ? Vector3.Lerp(cur, localPos, 1f - Mathf.Exp(-followLerp * Time.deltaTime))
                : localPos;
            transform.localPosition = next;
        }
        else
        {
            Vector3 worldTarget = ship.TransformPoint(localPos);
            transform.position = smooth
                ? Vector3.Lerp(transform.position, worldTarget, 1f - Mathf.Exp(-followLerp * Time.deltaTime))
                : worldTarget;
        }

        // Billboard to camera for 2D look (also in LateUpdate to avoid cam jitter)
        if (!cam) cam = Camera.main;
        if (cam)
            transform.rotation = Quaternion.LookRotation(-cam.transform.forward, Vector3.up);
    }
}
