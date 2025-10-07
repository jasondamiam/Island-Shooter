using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;
    public LayerMask hitMask; // stel lagen in die geraakt mogen worden (Player, Default, etc.)

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Damage als er een Health component is
        var h = collision.collider.GetComponentInParent<HealthBar>();
        if (h != null)
        {
            h.TakeDamage((int)damage);
        }

        Destroy(gameObject);
    }
}
