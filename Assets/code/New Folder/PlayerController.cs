using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Playercontrols : MonoBehaviour
{
    // THise veriballs are for controller and controller animtations + grounde effect
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 500F;
    [Header("Ground Cheack Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    // Thise veribals are for fighting animations
    //  private Animator anim;
    [SerializeField] public float cooldownTime = 2f;
    private float nextFireTime = 0f;
    public static int noOfClicks = 0;
    float lastClikcedTime = 0;
    float maxComboDelay = 1;



    public AttributesManager attributes;

    //verbial data
    [SerializeField] int Attributes, health;


    bool isGrounded;

    float ySpeed;

    Quaternion targetRotation;

    CameraController cameraController;
    Animator animator;
    CharacterController characterController;

    private void Awake()
    {

        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }
    private void Update()
    {
        //test butten
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key was Pressed");

        }

        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit1"))
        {
            animator.SetBool("hit1", false);
        }
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit2"))
        {
            animator.SetBool("hit2", false);
        }
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f && animator.GetCurrentAnimatorStateInfo(0).IsName("hit3"))
        {
            animator.SetBool("hit3", false);
            noOfClicks = 0;
        }
        if (Time.time - lastClikcedTime > maxComboDelay)
        {
            noOfClicks = 0;

        }
        if (Time.time > nextFireTime)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                OnClick();
            }
        
        }


        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");



        float moveAmount = Mathf.Clamp01( Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;


        var moveDir = cameraController.GetPlanarRotation() * moveInput;

        GroundCheck();
       // Debug.Log("isGrounded = " + isGrounded);
        if (isGrounded)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
            var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;
        

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {

            
            
            targetRotation = Quaternion.LookRotation(moveDir);

            Quaternion hoi = cameraController.GetPlanarRotation();
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.2f, Time.deltaTime);
        

    }

    void GroundCheck() 
    
    {
       isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
    void OnClick()
    {
        lastClikcedTime = Time.time;
        noOfClicks++;
        if (noOfClicks == 1)
        {
            animator.SetBool("hit1", true);
        }
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);
        if(noOfClicks >= 2 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7 && animator.GetCurrentAnimatorStateInfo(0).IsName("hit1"))
        {
            animator.SetBool("hit1", false);
            animator.SetBool("hit2", true);
        }
        if (noOfClicks >= 3 && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7 && animator.GetCurrentAnimatorStateInfo(0).IsName("hit2"))
        {
            animator.SetBool("hit2", false);
            animator.SetBool("hit3", true);
        }
    }
}