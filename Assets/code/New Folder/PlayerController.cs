using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerControls : MonoBehaviour
{
    // ---------- Movement ----------
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 500f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, -0.9f, 0f);
    [SerializeField] private LayerMask groundLayer;

    // ---------- Combat (Combo) ----------
    [Header("Combat")]
    [SerializeField] private float cooldownTime = 2f;   // tijd tussen combos
    [SerializeField] private float maxComboDelay = 1f;  // max tijd tussen klikken

    // Gebruik je animator bools "hit1", "hit2", "hit3"
    private static int noOfClicks = 0;
    private float lastClickedTime = 0f;
    private float nextFireTime = 0f;

    // ---------- Health ----------
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar; // koppel in Inspector

    // ---------- Refs ----------
    [Header("References")]
    public AttributesManager attributes; // optioneel
    private CameraController cameraController;
    private Animator animator;
    private CharacterController characterController;

    // ---------- State ----------
    private bool isGrounded;
    private float ySpeed;
    private Quaternion targetRotation;

    private void Awake()
    {
        cameraController = Camera.main ? Camera.main.GetComponent<CameraController>() : null;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        targetRotation = transform.rotation; // veilige start
    }

    private void Start()
    {
        // Health init
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 1, maxHealth);
        if (healthBar) healthBar.SetMaxHealth(maxHealth);
        if (healthBar) healthBar.SetHealth(currentHealth);
    }

    private void Update()
    {
        HandleGround();
        HandleMovementAndRotation();
        HandleCombat();
        // testdamage (optioneel)
        if (Input.GetKeyDown(KeyCode.P)) TakeDamage(20);
    }

    // ----------------------------- Movement -----------------------------
    private void HandleGround()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        if (isGrounded)
            ySpeed = -0.5f; // klein beetje omlaag drukken
        else
            ySpeed += Physics.gravity.y * Time.deltaTime;
    }

    private void HandleMovementAndRotation()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = new Vector3(h, 0f, v).normalized;
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        // Planar richting via camera
        Quaternion planarRot = cameraController ? cameraController.GetPlanarRotation() : Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up));
        Vector3 moveDir = planarRot * moveInput;

        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        // Rotatie alleen als we bewegen
        if (moveAmount > 0.001f && moveDir.sqrMagnitude > 0.0001f)
            targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Animator parameter
        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
    }

    // ----------------------------- Combat (Combo) -----------------------------
    private void HandleCombat()
    {
        // Reset combo als te lang gewacht
        if (Time.time - lastClickedTime > maxComboDelay)
            noOfClicks = 0;

        // Klik om combo te starten/volgen, mits cooldown voorbij
        if (Time.time > nextFireTime && Input.GetMouseButtonDown(0))
        {
            OnClick();
        }

        // Booleans automatisch uitzetten wanneer clip (grotendeels) klaar is
        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (st.normalizedTime > 0.7f && st.IsName("hit1")) animator.SetBool("hit1", false);
        if (st.normalizedTime > 0.7f && st.IsName("hit2")) animator.SetBool("hit2", false);
        if (st.normalizedTime > 0.7f && st.IsName("hit3"))
        {
            animator.SetBool("hit3", false);
            noOfClicks = 0; // einde combo
            nextFireTime = Time.time + cooldownTime; // cooldown na volledige combo
        }
    }

    private void OnClick()
    {
        lastClickedTime = Time.time;
        noOfClicks = Mathf.Clamp(noOfClicks + 1, 1, 3);

        // state info voor gates
        var st = animator.GetCurrentAnimatorStateInfo(0);

        // Eerste hit
        if (noOfClicks == 1 && !IsAnyHitActive())
        {
            animator.SetBool("hit1", true);
            return;
        }

        // Tweede hit: alleen doorgaan vanuit hit1 en op 70%+
        if (noOfClicks >= 2 && st.IsName("hit1") && st.normalizedTime > 0.7f)
        {
            animator.SetBool("hit1", false);
            animator.SetBool("hit2", true);
            return;
        }

        // Derde hit: alleen doorgaan vanuit hit2 en op 70%+
        if (noOfClicks >= 3 && st.IsName("hit2") && st.normalizedTime > 0.7f)
        {
            animator.SetBool("hit2", false);
            animator.SetBool("hit3", true);
            return;
        }
    }

    private bool IsAnyHitActive()
    {
        return animator.GetBool("hit1") || animator.GetBool("hit2") || animator.GetBool("hit3");
    }

    // ----------------------------- Health -----------------------------
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Abs(damage), 0, maxHealth);
        if (healthBar) healthBar.SetHealth(currentHealth);
        // TODO: dood/knockback/iframes etc.
    }

    // ----------------------------- Debug & Gizmos -----------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(collision.gameObject.name);
    }
}
