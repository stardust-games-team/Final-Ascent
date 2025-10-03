using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float life = 5f;

    void Start()
    {
        // if there's a Rigidbody, we assume velocity was set by GunFire
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.useGravity = false;

        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision c)
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // if you set the projectile collider to "Is Trigger"
        Destroy(gameObject);
    }
}
