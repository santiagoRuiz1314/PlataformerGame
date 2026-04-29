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

    [Header("Referenciacion")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController characterController;
    private Vector3 velocity;
    private float currentSpeed;
    private float yaw;

    private Vector3 externalVelocity = Vector3.zero;
    


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
