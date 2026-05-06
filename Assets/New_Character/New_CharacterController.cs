using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class New_Character : MonoBehaviour
{
    [Header("Movimiento")]
    public float WalkSpeed = 4f;
    public float SprintSpeed = 6f;
    public float jumpHeight = 2f;
    public float rotationSpeed = 10f;
    public float mouseSensitivity = 1f;
    public float gravity = -20f;

    [Header("Referenciacion")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController characterController;
    private Vector3 velocity;
    private float currentSpeed;
    private float yaw;

    private Vector3 externalVelocity = Vector3.zero;

    public bool IsMoving { get; private set; }
    public Vector2 CurrentInput { get; private set; }
    public bool IsGrounded{get; private set;}
    public float CurrentYaw => yaw;
    


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        updateAnimator();
    }
    void HandleMovement()
    {
        IsGrounded = characterController.isGrounded;
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        IsMoving = inputDirection.magnitude > 0.1f;

        Vector3 moveDirection = Vector3.zero; 

        if (IsMoving)
        {
            moveDirection = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * inputDirection;
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? SprintSpeed : WalkSpeed;
        }

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator?.SetBool("IsJumping", true);
        }
        velocity.y += gravity * Time.deltaTime;

        Vector3 finalmovement = (moveDirection * currentSpeed + externalVelocity);

        // 👇 esto es lo importante
        finalmovement.y = velocity.y;

        characterController.Move(finalmovement * Time.deltaTime);

        if(IsGrounded && velocity.y < 0f)
        {
            animator?.SetBool("IsJumping", false);
        }
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        yaw += mouseX;

        if (IsMoving)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0f, yaw, 0f),
                rotationSpeed * Time.deltaTime
            );
        }
    }
    void updateAnimator()
    {
        float SpeedPercent = IsMoving ? (currentSpeed == SprintSpeed ? 1f : 0.5f) : 0f;

        animator?.SetFloat("Speed", SpeedPercent, 0.1f, Time.deltaTime);
        animator?.SetBool("IsGrounded", IsGrounded);
        animator?.SetFloat("VerticalSpeed", velocity.y);
    }
   
}
