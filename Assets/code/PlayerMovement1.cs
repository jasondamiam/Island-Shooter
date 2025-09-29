using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float gravity = -9.81f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // 👈 Needs Animator component
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Set animation speed
        float currentSpeed = new Vector2(x, z).magnitude;
        animator.SetFloat("moveSpeed", currentSpeed);

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            animator.SetBool("isJumping", true);
            // (add jump force here)
        }
        else
        {
            animator.SetBool("isJumping", false);
        }

    }
}
