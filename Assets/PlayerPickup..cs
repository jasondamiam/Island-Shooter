using UnityEngine;
using System.Collections;

public class PlayerPickup : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCamera;          // je FPS camera
    public Transform hand;               // empty onder je camera (scale 1,1,1)
    public LayerMask pickupMask;

    [Header("Interact")]
    public float pickUpRange = 3f;
    public KeyCode pickKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;

    [Header("Snap animatie")]
    public float snapDuration = 0.08f;
    public AnimationCurve snapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private HoldableItem _current;
    private HoldableItem _hover;
    private Coroutine _snapRoutine;

    void Update()
    {
        UpdateHover();

        // Drop
        if (_current != null && Input.GetKeyDown(dropKey))
        {
            var carrierRb = GetComponent<Rigidbody>();
            Vector3 carrierVel = carrierRb ? carrierRb.linearVelocity : Vector3.zero;

            _current.Drop(carrierVel);
            _current = null;
            return;
        }

        // Pick-up
        if (_current == null && _hover != null && Input.GetKeyDown(pickKey))
        {
            _hover.PickUp(hand);
            _current = _hover;

            if (_snapRoutine != null) StopCoroutine(_snapRoutine);
            _snapRoutine = StartCoroutine(SnapToHand(_current));
        }
    }

    void UpdateHover()
    {
        _hover = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, pickupMask, QueryTriggerInteraction.Ignore))
        {
            var holdable = hit.collider.GetComponentInParent<HoldableItem>();
            if (holdable != null && !holdable.IsEquipped && _current == null)
            {
                _hover = holdable;
            }
        }
    }

    IEnumerator SnapToHand(HoldableItem item)
    {
        if (item == null || snapDuration <= 0f) yield break;

        Transform t = item.transform;

        // start met kleine overshoot voor �plop�
        Vector3 startPos = item.heldLocalPosition + new Vector3(0.02f, -0.02f, 0.04f);
        Quaternion startRot = Quaternion.Euler(item.heldLocalEuler + new Vector3(6f, -6f, 0f));
        Vector3 startScale = item.heldLocalScale * 1.06f;

        t.localPosition = startPos;
        t.localRotation = startRot;
        t.localScale = startScale;

        float tt = 0f;
        while (tt < 1f)
        {
            tt += Time.deltaTime / snapDuration;
            float k = snapCurve.Evaluate(Mathf.Clamp01(tt));

            t.localPosition = Vector3.Lerp(startPos, item.heldLocalPosition, k);
            t.localRotation = Quaternion.Slerp(startRot, Quaternion.Euler(item.heldLocalEuler), k);
            t.localScale = Vector3.Lerp(startScale, item.heldLocalScale, k);
            yield return null;
        }

        t.localPosition = item.heldLocalPosition;
        t.localRotation = Quaternion.Euler(item.heldLocalEuler);
        t.localScale = item.heldLocalScale;
    }
}
