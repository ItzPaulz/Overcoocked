using UnityEngine;
using System.Collections;

public class CannonShooter : MonoBehaviour
{
    [Header("Setup")]
    public Transform firePoint;         // Where the bullet leaves the cannon
    public float fireForce = 500f;
    public string enemyTag = "Enemy";   // Tag assigned to Micro_Sub prefabs

    [Tooltip("Extra offset to spawn the bullet slightly ahead of the barrel to avoid sticking inside the collider")]
    public float muzzleOffset = 0.2f;

    private bool hasFired;              // Prevent multiple shots per load

    private void OnTriggerEnter(Collider other)
    {
        if (hasFired) return;                           // Already fired
        if (!other.CompareTag("Plato")) return;         // Only accept bullet prefab

        if (other.TryGetComponent(out Rigidbody rb) && other.transform.parent == null && !rb.isKinematic)
        {
            Debug.Log("Cannon loaded, firing 🚀");

            // Reset physics
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Position slightly outside the barrel to avoid collision issues
            Vector3 spawnPos = firePoint.position + firePoint.forward * muzzleOffset;
            rb.transform.position = spawnPos;
            rb.transform.rotation = firePoint.rotation;

            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Ensure tag stays as "Plato"
            rb.gameObject.tag = "Plato";

            // Direction toward closest enemy (or forward if none)
            Vector3 dir = GetDirectionTowardClosestEnemy(spawnPos);

            rb.AddForce(dir * fireForce, ForceMode.Impulse);

            hasFired = true;   // Prevent double firing for same load
            StartCoroutine(ResetAfterDelay(0.5f)); // Allow next shot after delay
        }
    }

    Vector3 GetDirectionTowardClosestEnemy(Vector3 origin)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float minDistSqr = Mathf.Infinity;

        foreach (var e in enemies)
        {
            float distSqr = (e.transform.position - origin).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                closest = e.transform;
            }
        }

        return closest ? (closest.position - origin).normalized : firePoint.forward;
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hasFired = false;
    }
}


