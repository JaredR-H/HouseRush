using UnityEngine;

public class ObjectShooter : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject projectilePrefab;
    public Camera mainCamera; // Arrastra tu Main Camera aquí

    [Header("Configuración de Disparo")]
    public float shootForce = 20f;
    public float spawnDistance = 1.5f; // Distancia desde la cámara para que no aparezca "dentro" del lente

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Click izquierdo
        {
            ShootAtMouseClick();
        }
    }

    void ShootAtMouseClick()
    {
        if (projectilePrefab == null || mainCamera == null) return;

        // 1. Crear un rayo que va desde la cámara hacia el cursor
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        // 2. Determinar el punto de destino
        // Si el rayo golpea algo (suelo, pared, etc.)
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Si hacemos clic al cielo infinito, definimos un punto lejano
            targetPoint = ray.GetPoint(100);
        }

        // 3. Calcular la dirección desde el punto de aparición hacia el destino
        Vector3 spawnPos = mainCamera.transform.position + (ray.direction * spawnDistance);
        Vector3 shootDirection = (targetPoint - spawnPos).normalized;

        // 4. Instanciar y disparar
        GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Aplicamos la fuerza en la dirección calculada hacia el clic
            rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
        }

        Destroy(obj, 5f);
    }
}