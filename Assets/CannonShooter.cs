using UnityEngine;

public class CannonShooter : MonoBehaviour
{
    public Transform firePoint;              // Punto desde donde se dispara la bala
    public float fireForce = 500f;
    private bool hasFired = false; // Para evitar múltiples disparos

    private void OnTriggerEnter(Collider other)
    {
        if (hasFired) return;

        if (other.CompareTag("Plato"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            // ✅ Verifica si ya fue soltada (no tiene padre y no es kinematic)
            if (rb != null && other.transform.parent == null && !rb.isKinematic)
            {
                Debug.Log("¡Bala soltada dentro del cañón!");

                // Reposicionar al punto de disparo
                other.transform.position = firePoint.position;
                other.transform.rotation = firePoint.rotation;

                // Resetear física
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Disparar
                rb.AddForce(firePoint.forward * fireForce, ForceMode.Impulse);

                hasFired = true;
            }
        }
    }
}

