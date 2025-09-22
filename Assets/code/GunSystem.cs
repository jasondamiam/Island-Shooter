using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    //bools
    bool shooting, readyToShoot, reloading;

    //reference
    public Camera cam;
    public Transform attackPoints;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    //Graphics
    public GameObject muzzleFlash, bulletHoleGraphic;
    public TextMeshProUGUI text;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot= true;
    }

    private void Update()
    {
        MyInput();

        //SetText
        text.SetText(bulletsLeft +" / " + magazineSize);
    }

    private void MyInput() 
    
    {
        if (allowButtonHold) shooting = Input.GetMouseButton(0);
        else shooting = Input.GetMouseButtonDown(0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();
    
        // shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        //spread 
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-range, range);

        //Calculate direction with spread
        Vector3 direction = cam.transform.forward + new Vector3(x,y,0);


        //RayCast
        if (Physics.Raycast(cam.transform.position, direction, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);
            if (rayHit.collider.CompareTag("Enemy"));
             rayHit.collider.GetComponent<ShootingAi>().TakeDamage(damage);
        }

        //Graphics
        Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0, 180, 0));
        Instantiate(muzzleFlash, attackPoints.position, Quaternion.identity);
            
        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", timeBetweenShooting);
            
        if(bulletsShot > 0 && bulletsLeft > 0)
        Invoke("Shoot", timeBetweenShooting);
    }

        private void ResetShot()
        {
            readyToShoot=true;
        }

        private void Reload()
        {
            reloading = true;
        Invoke("ReloadingFinished", reloadTime);
        }

    private void ReloadFinished()
    { 
         bulletsLeft = magazineSize;
         reloading = false;

    }

}
