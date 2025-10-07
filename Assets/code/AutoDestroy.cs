using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("How long (in seconds) before this object destroys itself.")]
    public float lifetime = 2f;

    void Start()
    {
        // Automatically destroy the object after the given time
        Destroy(gameObject, lifetime);
    }

    // Optional: destroy early if the particle effect finishes
    void Update()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}