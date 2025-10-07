using System.Collections;
using TMPro;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    [Header("Gun Stats")]
    public int damage = 25;
    public float timeBetweenShooting = 0.1f;
    public float spread = 0.02f;
    public float range = 100f;
    public float reloadTime = 1.5f;
    public int magazineSize = 30;
    public int bulletsPerTap = 1;
    public bool allowButtonHold = true;

    private int bulletsLeft;
    private int bulletsShot;

    private bool shooting;
    private bool readyToShoot;
    private bool reloading;

    [Header("References")]
    public Camera cam;
    public Transform attackPoint;
    public LayerMask whatIsEnemy;
    public LayerMask whatIsEnvironment;

    [Header("Graphics")]
    public GameObject muzzleFlash;
    public GameObject bulletHoleGraphic;
    public GameObject bloodEffect;
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        HandleInput();

        if (ammoText != null)
            ammoText.text = $"{bulletsLeft} / {magazineSize}";
    }

    private void HandleInput()
    {
        shooting = allowButtonHold ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
            Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Add random spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 direction = (cam.transform.forward + new Vector3(x, y, 0f)).normalized;

        if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit, range))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                // Spawn blood effect
                if (bloodEffect != null)
                    Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));

                // Flash enemy color
                MonoBehaviour mb = damageable as MonoBehaviour;
                if (mb != null)
                    StartCoroutine(HitFlash(mb.gameObject));
            }
            else
            {
                // Spawn bullet hole on environment
                if (bulletHoleGraphic != null && ((1 << hit.collider.gameObject.layer) & whatIsEnvironment) != 0)
                {
                    var hole = Instantiate(bulletHoleGraphic, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
                    hole.transform.SetParent(hit.collider.transform);
                }
            }
        }

        // Muzzle flash
        if (muzzleFlash != null && attackPoint != null)
            Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);

        bulletsLeft--;
        bulletsShot--;

        Invoke(nameof(ResetShot), timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShooting);
    }

    private void ResetShot() => readyToShoot = true;

    private void Reload()
    {
        reloading = true;
        Invoke(nameof(FinishReload), reloadTime);
    }

    private void FinishReload()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private IEnumerator HitFlash(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = Color.white;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = Color.red;
        }
    }
}
