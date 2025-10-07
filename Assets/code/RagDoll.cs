using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    private Rigidbody[] rigidBodies;
    private Animator animator;

    void Start()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
        foreach (var rb in rigidBodies)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }

    public void ActivateRagdoll()
    {
        foreach (var rb in rigidBodies)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
        if (animator != null)
            animator.enabled = false;
    }
}
