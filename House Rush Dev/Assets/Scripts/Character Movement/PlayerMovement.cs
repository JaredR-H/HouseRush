using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 18f;
    public float rotationSpeed = 1200f;

    [Header("Detección de Capas")]
    // ESTA ES LA VARIABLE QUE TE FALTA:
    public LayerMask wallLayer;

    [Header("Ajustes de Empuje")]
    public float pushForce = 10f;

    private Rigidbody rb;
    private Vector2 inputVector;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            float moveX = (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed ? 1 : 0) -
                          (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed ? 1 : 0);

            float moveZ = (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed ? 1 : 0) -
                          (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed ? 1 : 0);

            inputVector = new Vector2(moveX, moveZ).normalized;
        }
    }

    void FixedUpdate()
    {
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        float currentSpeed = moveSpeed;

        if (moveDirection.magnitude > 0.1f)
        {
            RaycastHit hit;
            // Lanzamos el SweepTest para ver qué hay delante
            if (rb.SweepTest(moveDirection, out hit, (moveSpeed * Time.fixedDeltaTime) + 0.1f))
            {
                // Es una pared
                if (((1 << hit.collider.gameObject.layer) & wallLayer) != 0)
                {
                    // Deslizamos (tu código actual)
                    Vector3 movementStep = moveDirection * moveSpeed * Time.fixedDeltaTime;
                    movementStep = Vector3.ProjectOnPlane(movementStep, hit.normal);
                    rb.MovePosition(rb.position + movementStep);
                }
                // Es un objeto físico (Mueble)
                else
                {
                    // Si estamos muy cerca del mueble, bajamos la velocidad un 10% 
                    // solo en el momento del impacto para evitar que lo atraviese.
                    currentSpeed = moveSpeed * 0.5f;
                    rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime);
                }
            }
            else
            {
                // CASO C: No hay nada delante, movemos a velocidad total
                rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            }

            // Rotación
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    // Lógica de empuje constante para muebles
    private void OnCollisionStay(Collision collision)
    {
        Rigidbody targetBody = collision.collider.attachedRigidbody;

        if (targetBody != null && !targetBody.isKinematic)
        {
            Vector3 pushDir = collision.gameObject.transform.position - transform.position;
            pushDir.y = 0;
            pushDir.Normalize();

            targetBody.linearVelocity = pushDir * pushForce;
        }
    }
}