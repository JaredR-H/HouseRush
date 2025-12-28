using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f; // Ahora es un valor de fuerza
    public Vector3 offset;

    // Cambiamos a FixedUpdate para que la cámara se mueva al mismo ritmo que la física del Bean
    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Usamos Lerp pero con Time.fixedDeltaTime
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);
    }
}