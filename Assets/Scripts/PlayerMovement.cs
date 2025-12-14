using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Transform cameraTransform; // Arrastra tu Main Camera aquí
    public Transform groundCheck;     // Objeto vacío en los pies del personaje

    [Header("Configuración Base")]
    public float speed = 10f;
    public float rotationSpeed = 10f;
    public float maxSlopeAngle = 45f; // Ángulo máximo que puede subir

    [Header("Dash (Shift)")]
    public float dashForce = 20f;
    public float dashCooldown = 1f;
    private float lastDashTime;

    [Header("Patinaje (Ctrl)")]
    public float normalDrag = 5f;
    public float skatingDrag = 0.1f;

    [Header("Salto")]
    public float jumpForce = 5f; // Recuerda ajustar esto si subiste la gravedad global
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // Variables internas
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested = false;
    private bool isSkating = false;
    private bool dashRequested = false;

    // Variables para rampas
    private RaycastHit slopeHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Bloquear cursor al iniciar
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Autodetectar cámara si se olvida asignar
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                Debug.LogError("¡No se encontró ninguna cámara etiquetada como MainCamera!");
        }
    }

    // --- INPUT SYSTEM (Mensajes) ---
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

    // --- LÓGICA FÍSICA ---
    void FixedUpdate()
    {
        // 1. Detectar suelo
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 2. Calcular Dirección Relativa a la Cámara
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // 3. Lógica de Pendientes (Solución "Pro")
        if (OnSlope() && !jumpRequested)
        {
            // Proyectamos el movimiento a la inclinación de la rampa
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * speed, ForceMode.Force);

            // Si nos movemos en rampa, empujamos hacia abajo para no "saltar" al bajar
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                rb.AddForce(-slopeHit.normal * 80f, ForceMode.Force);
            }

            // Apagamos la gravedad normal para que no nos deslice hacia abajo
            rb.useGravity = false;
        }
        else
        {
            // Movimiento normal (Suelo plano o Aire)
            rb.AddForce(moveDirection * speed, ForceMode.Force);
            rb.useGravity = true;
        }

        // 4. Salto
        if (jumpRequested)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        // 5. Dash
        if (dashRequested)
        {
            // Si nos movemos, dash hacia allá. Si no, dash hacia el frente del personaje.
            Vector3 dashDir = moveDirection.magnitude > 0 ? moveDirection : transform.forward;

            // Ajustar dash a la pendiente si estamos en el suelo
            if (OnSlope()) dashDir = GetSlopeMoveDirection(dashDir);

            rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
            dashRequested = false;
        }

        // 6. Rotación del personaje
        if (moveDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

        // 7. Manejo de Fricción (Linear Damping)
        HandleDrag();
    }

    // --- FUNCIONES AUXILIARES ---

    void HandleDrag()
    {
        if (isSkating)
        {
            rb.linearDamping = skatingDrag; // Modo hielo/patinaje
        }
        else
        {
            if (isGrounded)
            {
                // En el suelo: Si me muevo, 0 fricción. Si suelto controles, freno fuerte.
                if (moveInput.magnitude > 0.1f)
                    rb.linearDamping = 0f;
                else
                    rb.linearDamping = normalDrag;
            }
            else
            {
                // En el aire: 0 fricción para caer rápido (Gravedad hace su trabajo)
                rb.linearDamping = 0f;
            }
        }
    }

    bool OnSlope()
    {
        if (!isGrounded) return false;

        // Lanzamos rayo hacia abajo
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundDistance + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            // Es pendiente si el ángulo > 0 y menor al máximo permitido
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}