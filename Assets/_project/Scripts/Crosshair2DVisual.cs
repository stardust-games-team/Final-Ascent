using UnityEngine;
using UnityEngine.Rendering;

public class Crosshair2DVisual : MonoBehaviour
{
    [Header("Camera-facing (2D look)")]
    public Camera cam;

    [Header("Appearance")]
    public Color color = Color.white;
    public float lineWidth = 0.04f;
    public bool showRing = true;
    public int ringSegments = 48;
    public float ringRadius = 0.35f;
    public bool showCross = true;
    public float tickLength = 0.5f;   // half-length of each line

    [Header("Height Lock")]
    public bool lockY = true;
    public float fixedY = 0.25f;

    [Header("Material (optional)")]
    public Material lineMaterial;     // leave empty to auto-create

    Transform ringT, hT, vT;
    LineRenderer ringLR, hLR, vLR;

    Material mat;

    void Start()
    {
        if (!cam) cam = Camera.main;
        EnsureMaterial();
        EnsureChildrenAndLRs();
        Rebuild();
    }

    void LateUpdate()
    {
        if (lockY)
        {
            var p = transform.position; p.y = fixedY; transform.position = p;
        }

        if (!cam) cam = Camera.main;
        if (cam)
            transform.rotation = Quaternion.LookRotation(-cam.transform.forward, Vector3.up);
    }

    void OnValidate()
    {
        ringSegments = Mathf.Max(8, ringSegments);
        lineWidth = Mathf.Max(0.001f, lineWidth);
        ringRadius = Mathf.Max(0.001f, ringRadius);
        tickLength = Mathf.Max(0.001f, tickLength);

        if (Application.isPlaying && mat != null)
            Rebuild();
    }

    // ---------- helpers ----------

    void EnsureMaterial()
    {
        if (lineMaterial != null) { mat = lineMaterial; return; }

        Shader sh = Shader.Find("Sprites/Default");
        if (!sh) sh = Shader.Find("Unlit/Color");
        if (!sh) sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (!sh) sh = Shader.Find("Hidden/Internal-Colored");

        mat = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
        mat.renderQueue = 3000;
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
    }

    Transform EnsureChild(string name)
    {
        var t = transform.Find(name);
        if (!t)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            t = go.transform;
        }
        return t;
    }

    LineRenderer EnsureLR(Transform t)
    {
        var lr = t.GetComponent<LineRenderer>();
        if (!lr) lr = t.gameObject.AddComponent<LineRenderer>();

        lr.useWorldSpace = false;                 // local space in child
        lr.alignment = LineAlignment.View;        // face camera
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.shadowCastingMode = ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sharedMaterial = mat;
        lr.sortingOrder = short.MaxValue;
        lr.widthMultiplier = lineWidth;
        return lr;
    }

    void EnsureChildrenAndLRs()
    {
        ringT = EnsureChild("Ring");
        hT = EnsureChild("LineH");
        vT = EnsureChild("LineV");

        ringLR = EnsureLR(ringT);
        hLR = EnsureLR(hT);
        vLR = EnsureLR(vT);
    }

    void Rebuild()
    {
        if (mat != null && mat.HasProperty("_Color")) mat.color = color;

        // Ring
        ringLR.enabled = showRing;
        ringLR.widthMultiplier = lineWidth;
        if (showRing)
        {
            int count = ringSegments + 1; // close loop
            ringLR.positionCount = count;
            float step = Mathf.PI * 2f / ringSegments;
            for (int i = 0; i < count; i++)
            {
                float a = i * step;
                ringLR.SetPosition(i, new Vector3(Mathf.Cos(a) * ringRadius, Mathf.Sin(a) * ringRadius, 0f));
            }
        }

        // Cross lines
        hLR.enabled = vLR.enabled = showCross;
        hLR.widthMultiplier = vLR.widthMultiplier = lineWidth;

        if (showCross)
        {
            hLR.positionCount = 2;
            hLR.SetPosition(0, new Vector3(-tickLength, 0f, 0f));
            hLR.SetPosition(1, new Vector3(+tickLength, 0f, 0f));

            vLR.positionCount = 2;
            vLR.SetPosition(0, new Vector3(0f, -tickLength, 0f));
            vLR.SetPosition(1, new Vector3(0f, +tickLength, 0f));
        }
    }
}
