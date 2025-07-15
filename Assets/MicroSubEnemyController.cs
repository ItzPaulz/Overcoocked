using UnityEngine;

public class MicroSubEnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform player;                  // Player transform inside StylShip_Unity
    public GameObject laserPrefab;            // Prefab with LaserController
    public GameObject explosionPrefab;        // FX on laser impact

    [Header("Movement / Orbit")]
    public float moveSpeed = 3f;              // Tangential speed
    public float orbitRadiusMin = 4f;
    public float orbitRadiusMax = 6f;
    public float radialAdjustSpeed = 2f;

    [Header("Combat")]
    public float fireRate = 2f;               // Seconds between shots
    public float laserSpeed = 12f;

    [Header("Duplication Settings")]
    public int maxDuplicates = 10;       // Global limit (originals + clones)
    public string bulletTag = "Plato"; // Tag of the projectile that triggers duplication
    public float spawnOffsetRadius = 2f;     // Distance from player when cloning

    /* ---------------- Internals ---------------- */
    private float _orbitRadius;
    private int _orbitDirection;            // 1 clockwise, -1 counter‑clockwise
    private float _fireTimer;
    private float _fixedY;                    // Altitude to maintain

    private static int _instanceCount;       // Global count
    private static bool _initialCloneSpawned; // Ensures only one auto‑clone

    /* ---------------- LIFECYCLE ---------------- */
    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        _orbitRadius = Random.Range(orbitRadiusMin, orbitRadiusMax);
        _orbitDirection = Random.value > 0.5f ? 1 : -1;
        _fireTimer = Random.Range(0f, fireRate);
        _fixedY = transform.position.y;

        _instanceCount++;

        if (!_initialCloneSpawned && _instanceCount < maxDuplicates)
        {
            SpawnCloneNear(player.position);
            _initialCloneSpawned = true;
        }
    }

    void OnDestroy() => _instanceCount--;

    void Update()
    {
        if (!player) return;
        MoveOrbit();
        HandleFiring();
    }

    /* ---------------- ORBIT MOVEMENT ---------------- */
    void MoveOrbit()
    {
        Vector3 toPlayer = transform.position - player.position;
        Vector3 horizontal = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = horizontal.magnitude;
        if (distance < 0.01f) return;

        Vector3 radialDir = horizontal.normalized;
        Vector3 tangentialDir = Vector3.Cross(Vector3.up * _orbitDirection, radialDir).normalized;

        Vector3 move = tangentialDir * moveSpeed;
        float radiusError = distance - _orbitRadius;
        move += radialDir * (-radiusError * radialAdjustSpeed);

        transform.position += move * Time.deltaTime;

        // Maintain altitude
        Vector3 pos = transform.position;
        pos.y = _fixedY;
        transform.position = pos;

        transform.rotation = Quaternion.LookRotation(-radialDir);
    }

    /* ---------------- SHOOTING ---------------- */
    void HandleFiring()
    {
        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            Shoot();
            _fireTimer = fireRate;
        }
    }

    void Shoot()
    {
        if (!laserPrefab) return;
        GameObject laserGO = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        if (laserGO.TryGetComponent(out LaserController laser))
            laser.Init(player, laserSpeed, explosionPrefab);
    }

    /* ---------------- DUPLICATION ON BULLET HIT ---------------- */
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(bulletTag)) return;
        Destroy(other.gameObject);

        if (_instanceCount >= maxDuplicates) return;
        SpawnCloneNear(player.position);
    }

    private void SpawnCloneNear(Vector3 origin)
    {
        Vector3 offset = Random.insideUnitSphere * spawnOffsetRadius;
        offset.y = 0f;
        Vector3 spawnPos = origin + offset;
        spawnPos.y = _fixedY;
        Instantiate(gameObject, spawnPos, Quaternion.identity);
    }
}