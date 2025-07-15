using UnityEngine;
using System.Collections;

public class CannonShooter : MonoBehaviour
{
    [Header("Setup")]
    public Transform firePoint;
    public float fireForce = 500f;
    public string enemyTag = "Enemy";

    [Tooltip("Distancia por delante del cañón donde aparece la bala")]
    public float muzzleOffset = .2f;

    [Header("Animación de carga")]
    public float loadDuration = .25f;           // tiempo que tarda en entrar
    public AnimationCurve loadCurve =           // trayectoria (0→1)
        AnimationCurve.EaseInOut(0,0,1,1);

    bool hasFired;

    /* ─────────────────────────────────────────────────────────────── */

    void OnTriggerEnter(Collider other)
    {
        if (hasFired)                    return;          // ya hay disparo
        if (!other.CompareTag("Plato"))  return;          // solo acepta balas

        // bala suelta + rigibody válido
        if (other.TryGetComponent(out Rigidbody rb) &&
            other.transform.parent == null && !rb.isKinematic)
        {
            StartCoroutine(LoadAndFire(rb));              // ← NUEVO
        }
    }

    /* ─────────────  CARGAR BALÍSTICO + DISPARO  ───────────── */

    IEnumerator LoadAndFire(Rigidbody rb)
    {
        hasFired          = true;
        rb.isKinematic    = true;              // congelamos mientras entra
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.tag            = "Plato";

        Vector3 startPos  = rb.position;
        Quaternion startRot = rb.rotation;
        Vector3 endPos    = firePoint.position + firePoint.forward * muzzleOffset;
        Quaternion endRot = firePoint.rotation;

        // Pequeña animación de entrada
        for (float t = 0; t < 1f; t += Time.deltaTime / loadDuration)
        {
            float k       = loadCurve.Evaluate(t);
            rb.position   = Vector3.Lerp(startPos, endPos, k);
            rb.rotation   = Quaternion.Slerp(startRot, endRot, k);
            yield return null;
        }

        // ¡DISPARO!
        rb.position    = endPos;
        rb.rotation    = endRot;
        rb.isKinematic = false;

        Transform enemy = FindClosestEnemy(endPos);
        Vector3 dir     = enemy ? (enemy.position - endPos).normalized
                                : firePoint.forward;

        Vector3 impulse = dir * fireForce;
        rb.AddForce(impulse, ForceMode.Impulse);

        // si la bala ya trae HomingBullet lo usamos; si no, lo añadimos
        GrabbableHomingBullet bullet = rb.GetComponent<GrabbableHomingBullet>();
        bullet.Init(enemy, impulse / rb.mass); // velocidad = impulso / masa

        yield return new WaitForSeconds(.5f);  // retardo para la siguiente bala
        hasFired = false;
    }

    /* ─────────────  UTILIDAD ───────────── */

    Transform FindClosestEnemy(Vector3 from)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float bestDist2   = Mathf.Infinity;

        foreach (var e in enemies)
        {
            float d2 = (e.transform.position - from).sqrMagnitude;
            if (d2 < bestDist2) { bestDist2 = d2; closest = e.transform; }
        }
        return closest;
    }
}
