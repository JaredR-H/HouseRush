using UnityEngine;

public class PhysicsPlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public FloatyPhysicsController floatyController;
    public Rigidbody rb;
    public Transform cameraTransform; // Arrastra tu Main Camera aquí

    [Header("Configuración de Movimiento")]
    public float maxSpeed = 8f;
    public float acceleration = 200f; // Qué tan rápido alcanza la velocidad máxima
    public float maxAccelForce = 150f; // Límite de fuerza para no salir volando

    [Header("Configuración de Rotación")]
    public float rotationSpeed = 10f;

    [Header("Configuración de Salto")]
    public float jumpForce = 15f;
    public float jumpCooldown = 0.2f;

    private Vector3 _moveInput;
    private float _lastJumpTime;

    void Update()
    {
        // 1. Inputs básicos (WASD)
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(x, 0, y).normalized;

        // 2. Salto (Solo si está en el suelo)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (floatyController.IsGrounded && Time.time > _lastJumpTime + jumpCooldown)
            {
                Jump();
            }
        }
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        // Solo rotamos si hay input de movimiento
        if (_moveInput.sqrMagnitude > 0.01f)
        {
            // Calculamos la dirección relativa a la cámara
            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 targetDir = (camForward * _moveInput.z + camRight * _moveInput.x);

            // Creamos la rotación hacia esa dirección
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);

            // Aplicamos la rotación suavemente usando Slerp
            // Importante: Esto rota el transform, pero como X y Z están bloqueados en el Rigidbody, 
            // el motor de física solo aceptará el cambio en el eje Y.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // 3. Lógica "No Air Control"
        // Si NO está en el suelo, NO aplicamos fuerzas de movimiento horizontal.
        if (floatyController.IsGrounded)
        {
            Move();
        }

        // Cuando el personaje se mueve, aplicamos un torque pequeño hacia atrás 
        // para que parezca que la aceleración lo inclina.
        Vector3 leanForce = new Vector3(_moveInput.z, 0, -_moveInput.x) * 5f;
        rb.AddTorque(leanForce, ForceMode.Acceleration);
    }

    private void Move()
    {
        // Calcular la dirección basada en la cámara
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 targetDir = (camForward * _moveInput.z + camRight * _moveInput.x);

        // --- AQUÍ ESTÁ LA MAGIA ANTI-HIELO (THE RUN AROUND) ---

        // 1. Calculamos la velocidad que QUEREMOS tener
        Vector3 targetVelocity = targetDir * maxSpeed;

        // 2. Obtenemos la velocidad ACTUAL (solo en plano horizontal)
        Vector3 currentVelocity = rb.linearVelocity; // O rb.velocity
        currentVelocity.y = 0; // Ignoramos la velocidad vertical (caída/salto)

        // 3. Calculamos la diferencia (Delta)
        Vector3 velError = targetVelocity - currentVelocity;

        // 4. Aplicamos fuerza proporcional al error (P-Controller de velocidad)
        // Esto actúa como acelerador Y como freno. Si targetVelocity es 0, frenará.
        Vector3 force = velError * acceleration;

        // Limitamos la fuerza para evitar tirones locos
        force = Vector3.ClampMagnitude(force, maxAccelForce);

        rb.AddForce(force, ForceMode.Force);
    }

    private void Jump()
    {
        // Resetear velocidad vertical para salto consistente
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;

        // Impulso hacia arriba
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _lastJumpTime = Time.time;
    }
}