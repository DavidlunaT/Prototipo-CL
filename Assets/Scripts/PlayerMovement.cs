using UnityEngine;
using UnityEngine.InputSystem; // 1. Necesario para el nuevo sistema

public class PlayerMovement : MonoBehaviour
{
    // VARIABLES: Las "perillas" que ajustarás en el Inspector
    public float speed = 10f; // Magnitud de la fuerza

    private Rigidbody rb;
    private Vector2 moveInput; // Aquí guardaremos la dirección (X, Y) del teclado

    // Awake se ejecuta una sola vez al nacer el objeto. Ideal para referencias.
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ESTO ES MAGIA DEL INPUT SYSTEM
    // El componente Player Input busca métodos que se llamen "On" + NombreDeLaAccion
    // Como la acción por defecto se llama "Move", él busca "OnMove".
    void OnMove(InputValue value)
    {
        // Leemos el vector (ej: W = (0, 1), D = (1, 0))
        moveInput = value.Get<Vector2>();
    }

    // FixedUpdate es como Update, pero sincronizado con el motor de FÍSICAS.
    // SIEMPRE mueve Rigidbodies aquí, nunca en Update.
    void FixedUpdate()
    {
        // Convertimos el input 2D (X, Y) a movimiento 3D (X, 0, Z)
        // Porque en 3D, el suelo es el plano X-Z. La Y es la altura.
        Vector3 forceDirection = new Vector3(moveInput.x, 0, moveInput.y);

        // Aplicamos fuerza física
        // ForceMode.Force = Empuje continuo (como un motor)
        rb.AddForce(forceDirection * speed, ForceMode.Force);
    }
}