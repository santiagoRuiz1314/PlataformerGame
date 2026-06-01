using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class New_Character : MonoBehaviour
{
    [Header("Movimiento")]
    public float WalkSpeed = 4f;
    public float SprintSpeed = 6f;
    public float jumpHeight = 2f;
    public float rotationSpeed = 10f;
    public float gravity = -20f;
    // Lo controla PlayerPowerUps. 1 = velocidad normal.
    [HideInInspector] public float SpeedMultiplier = 1f;

    [Header("Ejes (LN2 - cámara con rotación Y=-90)")]
    [Tooltip("Si está activo, el input vertical mueve al personaje en profundidad (X mundo).")]
    public bool allowDepthMovement = true;

    [Header("Referenciación")]
    public Animator animator;

    private CharacterController characterController;
    private Vector3 velocity;
    private Vector3 lastMoveDirection;
    private float currentSpeed;
    private float yaw;
    private Vector3 externalVelocity = Vector3.zero;

    public bool IsMoving { get; private set; }
    public Vector2 CurrentInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public float CurrentYaw => yaw;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateAnimator();
    }

    void HandleMovement()
    {
        IsGrounded = characterController.isGrounded;
        if (IsGrounded && velocity.y < 0f)
            velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        CurrentInput = new Vector2(horizontal, vertical);

        // Mapeo de ejes para cámara LN2 con rotación Y=-90:
        //   Horizontal → Z mundo (lateral en pantalla)
        //   Vertical   → X mundo (profundidad)
        float depth = allowDepthMovement ? -vertical : 0f;
        Vector3 inputDirection = new Vector3(depth, 0f, horizontal);
        if (inputDirection.sqrMagnitude > 1f) inputDirection.Normalize();

        IsMoving = inputDirection.magnitude > 0.1f;

        Vector3 moveDirection = Vector3.zero;
        if (IsMoving)
        {
            moveDirection = inputDirection;
            lastMoveDirection = inputDirection;
            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = (isSprinting ? SprintSpeed : WalkSpeed) * SpeedMultiplier;
        }

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator?.SetBool("IsJumping", true);
        }
        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = moveDirection * currentSpeed + externalVelocity;
        finalMovement.y = velocity.y;

        characterController.Move(finalMovement * Time.deltaTime);

        if (IsGrounded && velocity.y < 0f)
            animator?.SetBool("IsJumping", false);
    }

    void HandleRotation()
    {
        // Cámara fija: el personaje rota para mirar hacia donde se mueve.
        if (IsMoving && lastMoveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
            yaw = transform.eulerAngles.y;
        }
    }

    void UpdateAnimator()
    {
        float SpeedPercent = IsMoving ? (currentSpeed == SprintSpeed ? 1f : 0.5f) : 0f;
        animator?.SetFloat("Speed", SpeedPercent, 0.1f, Time.deltaTime);
        animator?.SetBool("IsGrounded", IsGrounded);
        animator?.SetFloat("VerticalSpeed", velocity.y);
    }

    public void SetExternalVelocity(Vector3 platformVelocity)
    {
        externalVelocity = platformVelocity;
    }
}