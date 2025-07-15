using UnityEngine;

/// <summary>
/// Bala que puede agarrar el jugador y, una vez disparada, persigue al enemigo.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class GrabbableHomingBullet : GrabbableObject
{
    /* ░░░░░░░░░ 1)  PROPIEDADES DE AGARRE ░░░░░░░░░ */

    public bool canGrab = false;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;

    JuicyObject juicy;       // highlight
    PlayerController holder; // quién la tiene en la mano

    /* ░░░░░░░░░ 2)  PROPIEDADES DE HOMING ░░░░░░░░░ */

    [Header("Movimiento (modo vuelo)")]
    public float cruiseSpeed       = 18f;   // velocidad constante
    [Range(0,1)] public float turnRate = .25f;
    public float lifeTime          = 5f;    // se autodestruye
    public float spinSpeed         = 720f;  // efecto visual

    Rigidbody rb;
    Transform target;
    bool isFlying;                          // ¿ya fue disparada?

    /* ────────────────────────────────────────── */

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        juicy = GetComponent<JuicyObject>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    /* =============================================================
     *  MODO AGARRE
     * ===========================================================*/

    void OnTriggerEnter(Collider other)
    {
        if (isFlying) return;                       // ya no se puede agarrar

        if (other.CompareTag("Player"))
        {
            canGrab = true;
            holder  = other.GetComponentInParent<PlayerController>();

            if (holder && !holder.HasObject((GrabbableObject)this))
                holder.SetGrabbableObject((GrabbableObject)this);

            juicy?.ShowHighlight();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isFlying) return;

        if (other.CompareTag("Player"))
        {
            canGrab = false;

            if (holder)
                holder.ClearGrabbableObject();

            holder = null;
            juicy?.HideHighlight();
        }
    }

    /* =============================================================
     *  MODO VUELO  (se activa con Init)
     * ===========================================================*/

    /// <summary>Llamado desde CannonShooter justo al disparar.</summary>
    public void Init(Transform enemy, Vector3 initialVelocity)
    {
        // Pasamos a modo vuelo
        isFlying   = true;
        canGrab    = false;
        target     = enemy;

        // Asegura que el Rigidbody esté libre
        rb.isKinematic = false;
        rb.linearVelocity    = initialVelocity;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Oculta cualquier highlight
        juicy?.HideHighlight();

        // Seguridad
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (!isFlying || !target) return;

        // Dir. deseada
        Vector3 desiredDir = (target.position - rb.position).normalized;
        Vector3 desiredVel = desiredDir * cruiseSpeed;

        // Suavizado
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVel, turnRate);

        // Orientar la bala
        if (rb.linearVelocity.sqrMagnitude > .01f)
            rb.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }

    void Update()
    {
        if (isFlying)
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision col)
    {
        if (!isFlying) return;  // ignorar choques mientras está en el piso

        // Aquí: partículas, daño, etc.
        Destroy(gameObject);
    }
}
