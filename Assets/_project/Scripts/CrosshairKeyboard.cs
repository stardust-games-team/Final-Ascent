using UnityEngine;

public class CrosshairKeyboard : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;          // units/second
    public float fixedY = 0.25f;           // keep slightly above ground

    [Header("Camera")]
    public Camera cam;                     // drag your Main Camera here
    public bool cameraRelative = true;     // W/A/S/D relative to camera view

    [Header("Play Area Bounds")]
    public Vector2 boundsX = new Vector2(-90f, 90f);
    public Vector2 boundsZ = new Vector2(-90f, 90f);

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        // raw input (WASD)
        float x = 0f, z = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;

        Vector3 moveDir = Vector3.zero;

        if (cameraRelative && cam)
        {
            // Horizontal camera axes projected onto XZ plane.
            // Note: in a straight top-down camera, cam.forward points down (Y-), so use cam.up as "screen up".
            Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            Vector3 camUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;

            moveDir = (camRight * x + camUp * z);
        }
        else
        {
            // World-relative (X/Z)
            moveDir = new Vector3(x, 0f, z);
        }

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize(); // diagonals

        Vector3 pos = transform.position + moveDir * moveSpeed * Time.deltaTime;

        // clamp & lock height
        pos.x = Mathf.Clamp(pos.x, boundsX.x, boundsX.y);
        pos.z = Mathf.Clamp(pos.z, boundsZ.x, boundsZ.y);
        pos.y = fixedY;

        transform.position = pos;
    }
}
