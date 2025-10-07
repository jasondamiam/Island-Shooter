using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Target")]
    public Transform player;                // sleep je Player hier in (bv. Player transform)

    [Header("Combat")]
    public GameObject bulletPrefab;         // prefab met Rigidbody + Bullet script
    public Transform muzzle;                // waar de kogel spawnt (empty child)
    public float fireCooldown = 0.5f;
    public float bulletSpeed = 20f;
    public float damagePerHit = 10f;

    [Header("Detection")]
    public float detectionRange = 15f;
    [Range(0f, 180f)] public float fieldOfView = 90f; // half-angle (90 = 180 totaal)
    public LayerMask losMask;               // obstakels (bijv. Default + Environment)

    [Header("Aiming")]
    public float turnSpeed = 8f;            // hoe snel draait de vijand

    private float _nextFireTime;

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > detectionRange) return; // te ver weg

        // Is speler in FOV?
        Vector3 fwd = transform.forward;
        Vector3 dir = toPlayer.normalized;
        float angle = Vector3.Angle(fwd, dir);
        if (angle > fieldOfView) return;

        // Smooth roteren naar speler (alleen horizontaal)
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        // Line of sight check (optioneel maar aanbevolen)
        if (!HasLineOfSight()) return;

        // Schiet met cooldown
        if (Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + fireCooldown;
        }
    }

    bool HasLineOfSight()
    {
        Vector3 start = muzzle ? muzzle.position : transform.position + Vector3.up * 1.5f;
        Vector3 end = player.position + Vector3.up * 1.0f; // richt op borst/hoofd
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);

        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, losMask, QueryTriggerInteraction.Ignore))
        {
            // alleen schieten als we de speler raken
            return hit.collider.transform.IsChildOf(player) || hit.collider.transform == player;
        }
        // niets geraakt = vrij zicht
        return true;
    }

    void Fire()
    {
        if (bulletPrefab == null || muzzle == null) return;

        GameObject b = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        Rigidbody rb = b.GetComponent<Rigidbody>();
        Bullet bullet = b.GetComponent<Bullet>();
        if (bullet != null) bullet.damage = damagePerHit;

        if (rb != null)
        {
            rb.linearVelocity = muzzle.forward * bulletSpeed;   // in nieuwe Unity versies
            // als jouw versie geen linearVelocity kent:
            // rb.velocity = muzzle.forward * bulletSpeed;
        }
    }

    // Debug gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
