using UnityEngine;

public class Targetable : MonoBehaviour
{
    void OnEnable()
    {
        // Check if UIManager exists before trying to add target
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddTarget(transform);
        }
    }

    void OnDisable()
    {
        RemoveTarget();
    }

    void OnDestroy()
    {
        RemoveTarget();
    }

    void RemoveTarget()
    {
        // Check if UIManager exists before trying to remove target
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveTarget(transform);
        }
    }
}