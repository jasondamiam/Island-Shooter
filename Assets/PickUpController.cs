using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, gunContainer, fpsCam;

    public float pickUpRange = 3f;
    public float dropForwardForce = 2f, dropUpwardForce = 1f;

    // Stel in via Inspector hoe het item in handen moet staan
    public Vector3 heldLocalPosition = Vector3.zero;
    public Vector3 heldLocalEuler = Vector3.zero;
    public Vector3 heldLocalScale = Vector3.one; // NIET forceren naar one als je prefab klein is

    public bool equipped;
    public static bool slotFull;

    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;

        if (!equipped)
        {
            rb.isKinematic = false;
            coll.isTrigger = false;
        }
        else
        {
            rb.isKinematic = true;
            coll.isTrigger = true;
            slotFull = true;
        }
    }

    private void Update()
    {
        Vector3 distanceToPlayer = player.position - transform.position;

        if (!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(KeyCode.N) && !slotFull)
            PickUp();

        if (equipped && Input.GetKeyDown(KeyCode.Q))
            Drop();
    }

    private void PickUp()
    {
        equipped = true;
        slotFull = true;

        // Parent naar container, NIET world pos behouden (false)
        transform.SetParent(gunContainer, false);

        // Plaats/orientatie/schaal instellen voor in-hand view
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEuler);
        transform.localScale = heldLocalScale; // kies passend formaat, bv 0.05f,0.05f,0.05f

        rb.isKinematic = true;
        coll.isTrigger = true;
    }

    private void Drop()
    {
        equipped = false;
        slotFull = false;

        // Losmaken; wereldpositie behouden (true) is hier prima
        transform.SetParent(null, true);

        // Originele schaal terugzetten
        transform.localScale = _originalScale;

        rb.isKinematic = false;
        coll.isTrigger = false;

        // Gebruik standaard Rigidbody velocity
        rb.linearVelocity = player.GetComponent<Rigidbody>().linearVelocity;

        rb.AddForce(player.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(player.up * dropUpwardForce, ForceMode.Impulse);

        float random = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(random, random, random) * 10f, ForceMode.Impulse);
    }
}
