using UnityEngine;

public class FloatyPhysicsController : MonoBehaviour
{
    [Header("Referencias")]
    public Rigidbody rb;

    [Header("Suspensión (Flotar)")]
    public float rideHeight = 1.3f;
    public float rideSpringStrength = 150f;
    public float rideSpringDamper = 10f;

    [Header("Equilibrio (Upright Force)")]
    public float uprightSpringStrength = 100f; // Súbelo si el personaje se cae mucho
    public float uprightSpringDamper = 10f;

    [Header("Gravedad Personalizada")]
    public float fallGravityMultiplier = 2.5f;

    public bool IsGrounded { get; private set; }
    private RaycastHit _rayHit;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        CheckGround();
        ApplySuspensionForce();
        ApplyUprightForce();
        ApplyExtraGravity();
    }

    private void CheckGround()
    {
        Vector3 rayDir = -transform.up;
        IsGrounded = Physics.Raycast(transform.position, rayDir, out _rayHit, rideHeight + 0.5f);
    }

    private void ApplySuspensionForce()
    {
        if (!IsGrounded) return;

        Vector3 vel = rb.linearVelocity;
        Vector3 rayDir = -transform.up;

        Vector3 otherVel = Vector3.zero;
        Rigidbody hitBody = _rayHit.rigidbody;
        if (hitBody != null) otherVel = hitBody.linearVelocity;

        float rayDirVel = Vector3.Dot(rayDir, vel);
        float otherDirVel = Vector3.Dot(rayDir, otherVel);
        float relVel = rayDirVel - otherDirVel;

        float x = _rayHit.distance - rideHeight;
        float springForce = (x * rideSpringStrength) - (relVel * rideSpringDamper);

        rb.AddForce(rayDir * springForce);

        if (hitBody != null)
        {
            hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
        }
    }

    private void ApplyUprightForce()
    {
        Quaternion characterCurrent = transform.rotation;

        // 1. Definimos el objetivo: Mantener el Y actual, pero resetear X y Z a 0
        Vector3 currentEuler = characterCurrent.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(0, currentEuler.y, 0);

        // 2. Calculamos la rotación más corta hacia ese objetivo específico
        // Usamos el método de Toyful para evitar giros extraños
        Quaternion toGoal = UtilsMath.ShortestRotation(targetRotation, characterCurrent);

        Vector3 rotAxis;
        float rotDegrees;

        // 3. Extraemos el eje y el ángulo para el torque
        toGoal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        float rotRadians = rotDegrees * Mathf.Deg2Rad;

        // 4. Aplicamos la fórmula de muelle de torsión (PD Controller)
        Vector3 torque = (rotAxis * (rotRadians * uprightSpringStrength)) - (rb.angularVelocity * uprightSpringDamper);

        // Usamos Acceleration para que ignore la masa y sea reactivo
        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    private void ApplyExtraGravity()
    {
        if (!IsGrounded)
        {
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private Quaternion ShortestRotation(Quaternion a, Quaternion b)
    {
        if (Quaternion.Dot(a, b) < 0) return a * Quaternion.Inverse(Multiply(b, -1));
        return a * Quaternion.Inverse(b);
    }

    private Quaternion Multiply(Quaternion input, float scalar)
    {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }


}