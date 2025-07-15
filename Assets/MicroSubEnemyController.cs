using UnityEngine;

public class EnemyChaseShoot : MonoBehaviour
{
    /* ---------- Referencias ---------- */
    [Header("Referencias")]
    public Transform player;            // Objetivo a perseguir
    public GameObject laserPrefab;      // Prefab del láser (con Rigidbody)

    /* ---------- Movimiento ---------- */
    [Header("Movimiento")]
    public float moveSpeed   = 8f;      // m/s - Velocidad de persecución
    public float fixedHeight = 12.8f;   // Altura fija sobre el suelo (Y mundial)
    public float stopDistance = 15f;    // Distancia mínima al jugador (para no chocar)
    
    [Header("Órbita")]
    public Vector3 targetPosition = new Vector3(-3.52f, 12.8f, -14.55f); // Posición objetivo
    public float orbitRadius = 30f;     // Radio de órbita
    public float orbitSpeed = 45f;      // Velocidad de órbita en grados/segundo
    public float arrivalThreshold = 2f; // Distancia para considerar que llegó
    
    private bool hasArrivedAtTarget = false;
    private float orbitAngle = 0f;

    /* ---------- Combate ---------- */
    [Header("Combate")]
    public float fireRate       = 2f;   // Segundos entre disparos (más frecuente)
    public float laserSpeed     = 20f;  // m/s (más rápido)
    public float detectionRange = 30f;  // Solo dispara si el jugador está cerca

    float _fireTimer;

    /* ---------- Inicialización ---------- */
    void Start()
    {
        // Dar a cada nave un ángulo inicial aleatorio para evitar que se agrupen
        orbitAngle = Random.Range(0f, 360f);
    }

    /* ---------- Bucle ---------- */
    void Update()
    {
        if (!player) return;

        if (!hasArrivedAtTarget)
        {
            MoveToTarget();
        }
        else
        {
            OrbitAroundTarget();
        }
        
        HandleShooting();
    }

    /* ---------- Movimiento a Posición Objetivo ---------- */
    void MoveToTarget()
    {
        // Calcular dirección hacia la posición objetivo
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f; // Mantener movimiento horizontal
        
        float distanceToTarget = dir.magnitude;
        
        // Verificar si llegó al objetivo
        if (distanceToTarget <= arrivalThreshold)
        {
            hasArrivedAtTarget = true;
            // Inicializar ángulo de órbita basado en la posición actual
            Vector3 toCenter = transform.position - targetPosition;
            toCenter.y = 0f;
            orbitAngle = Mathf.Atan2(toCenter.z, toCenter.x) * Mathf.Rad2Deg;
            return;
        }
        
        // Moverse hacia el objetivo
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        
        // Mantener altura constante
        Vector3 pos = transform.position;
        pos.y = fixedHeight;
        transform.position = pos;
    }
    
    /* ---------- Órbita Alrededor del Objetivo ---------- */
    void OrbitAroundTarget()
    {
        // Actualizar ángulo de órbita
        orbitAngle += orbitSpeed * Time.deltaTime;
        if (orbitAngle >= 360f) orbitAngle -= 360f;
        
        // Calcular nueva posición en órbita
        float radians = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = new Vector3(
            targetPosition.x + Mathf.Cos(radians) * orbitRadius,
            fixedHeight,
            targetPosition.z + Mathf.Sin(radians) * orbitRadius
        );
        
        transform.position = orbitPos;
        
        // Mirar hacia el jugador mientras orbita
        if (player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }

    /* ---------- Disparo ---------- */
    void HandleShooting()
    {
        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;

        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
            ShootLaser();

        _fireTimer = fireRate;
    }

    void ShootLaser()
    {
        if (!laserPrefab) return;

        Quaternion aim = Quaternion.LookRotation(player.position - transform.position);
        GameObject laser = Instantiate(laserPrefab, transform.position, aim);

        if (laser.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = laser.transform.forward * laserSpeed;

        Destroy(laser, 5f);
    }

    /* ---------- Colisión ---------- */
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plato"))
            Destroy(gameObject);
    }
}
