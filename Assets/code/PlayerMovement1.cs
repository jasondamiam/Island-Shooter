using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // -------- Movement --------
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    // -------- Health --------
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private HealthBar healthBar;  // koppel in Inspector

    // -------- Combat / Anim --------
    [Header("Animation & Combat")]
    [SerializeField] private string animParamMove = "moveSpeed";
    [SerializeField] private string animParamIsJumping = "isJumping";
    [SerializeField] private string animTriggerAttack = "attack";

    // -------- Internals --------
    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private int currentHealth;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // mag null zijn als je (nog) geen animator gebruikt
    }

    void Start()
    {
        // Health init
        currentHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    void Update()
    {
        HandleMovement();
        HandleJumpAndGravity();
        HandleAttack();

        // Test damage (optioneel): druk P om 20 damage te krijgen
        if (Input.GetKeyDown(KeyCode.P))
            TakeDamage(20);
    }

    // ---------------- Movement ----------------
    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Beweeg in lokale ruimte (voor FPS/TP karakter dat al naar camera/world kijkt)
        Vector3 move = (transform.right * x + transform.forward * z).normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Animator: 0..1 bewegingsintensiteit
        if (animator != null)
        {
            float moveAmount = Mathf.Clamp01(new Vector2(x, z).magnitude);
            animator.SetFloat(animParamMove, moveAmount, 0.15f, Time.deltaTime);
        }
    }

    // --------------- Jump & Gravity ---------------
    private void HandleJumpAndGravity()
    {
        // Reset kleine negatieve Y wanneer grounded zodat we stevig op de grond blijven
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Jump
        bool wantJump = Input.GetButtonDown("Jump") && controller.isGrounded;
        if (wantJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetBool(animParamIsJumping, true);
        }
        else if (animator != null)
        {
            // Zet jumping uit zodra we weer op de grond staan
            animator.SetBool(animParamIsJumping, !controller.isGrounded ? true : false);
        }

        // Gravity toepassen
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ---------------- Attack ----------------
    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && animator != null)
            animator.SetTrigger(animTriggerAttack);
    }

    // ---------------- Health ----------------
    public void TakeDamage(int damage)
    {
        int d = Mathf.Abs(damage);
        currentHealth = Mathf.Clamp(currentHealth - d, 0, maxHealth);
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        // TODO: speel death-animatie, disable input, respawn, etc.
        // Voor nu: simpel uitschakelen
        enabled = false;
        // Eventueel: animator?.SetTrigger("die");
        // Of Destroy(gameObject); als je dat wilt.
    }

    // ---------------- Public helpers ----------------
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
