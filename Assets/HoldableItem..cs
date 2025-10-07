using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class HoldableItem : MonoBehaviour
{
    [Header("In-hand transform")]
    public Vector3 heldLocalPosition = Vector3.zero;
    public Vector3 heldLocalEuler = Vector3.zero;
    public Vector3 heldLocalScale = Vector3.one; // Stel in op passend formaat (bv. 0.05,0.05,0.05)

    [Header("Drop forces")]
    public float dropForwardForce = 2f;
    public float dropUpwardForce = 1f;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Collider coll;

    // Originele staat opslaan om netjes te herstellen
    private Transform _originalParent;
    private Vector3 _originalLocalScale;
    private bool _equipped;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        _originalParent = transform.parent;
        _originalLocalScale = transform.localScale;
    }

    public bool IsEquipped => _equipped;

    public void PickUp(Transform handParent)
    {
        if (_equipped) return;
        _equipped = true;

        // Reparent zonder wereldcoords te behouden (voorkomt rare schaal)
        transform.SetParent(handParent, false);

        // Plaats/rotatie/schaal voor in de hand
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEuler);
        transform.localScale = heldLocalScale;

        // Physics uit
        rb.isKinematic = true;
        coll.isTrigger = true;
    }

    public void Drop(Vector3 carrierVelocity)
    {
        if (!_equipped) return;
        _equipped = false;

        // Losmaken – wereldpositie behouden
        transform.SetParent(null, true);

        // Originele schaal terug (je kunt ook _originalParent terugzetten als je wilt)
        transform.localScale = _originalLocalScale;

        // Physics aan
        rb.isKinematic = false;
        coll.isTrigger = false;

        // Snelheid & krachten
        rb.linearVelocity = carrierVelocity;
        rb.AddForce(transform.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(Vector3.up * dropUpwardForce, ForceMode.Impulse);

        // Klein beetje willekeurige spin
        float r = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(r, r, r) * 10f, ForceMode.Impulse);
    }
}
