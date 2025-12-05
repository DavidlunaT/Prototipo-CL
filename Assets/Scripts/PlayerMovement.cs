using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración Base")]
    
    public float speed = 10f; 
    public float rotationSpeed = 10f;

    [Header("Dash (Shift)")]
    public float dashForce = 20f; 
    public float dashCooldown = 1f; 
    private float lastDashTime;

    [Header("Patinaje (Ctrl)")]
    public float normalDrag = 5f;  
    public float skatingDrag = 0.1f;

    [Header("Salto")]
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask; 


    private bool isGrounded; 
    private bool jumpRequested = false;
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isSkating = false; 
    private bool dashRequested = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            jumpRequested = true;
        }
    }
    void OnSkate(InputValue value)
    {
        isSkating = value.isPressed;
    }
    void OnDash(InputValue value)
    {
        if (value.isPressed && Time.time >= lastDashTime + dashCooldown)
        {
            dashRequested = true;
            lastDashTime = Time.time; 
        }
    }

    void FixedUpdate()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);


        Vector3 forceDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        rb.AddForce(forceDirection * speed, ForceMode.Force);

        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
        if (forceDirection.magnitude > 0)
        {
            Quaternion nextRotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(forceDirection),rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(nextRotation);
        }
        
        if (dashRequested)
        {

            Vector3 dashDir = forceDirection.magnitude > 0 ? forceDirection : transform.forward;

            rb.AddForce(dashDir * dashForce, ForceMode.Impulse);

            dashRequested = false; 
        }


        if (isSkating)
        {
            rb.linearDamping = skatingDrag; 
        }
        else
        {
            rb.linearDamping = normalDrag;
        }
    }
}