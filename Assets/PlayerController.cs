using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;   // Para recargar la escena al morir

public class PlayerController : MonoBehaviour
{
    /* ───────────── NUEVO: SALUD ───────────── */
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;
    public GameObject deathEffectPrefab;   // Efecto de explosión/humo al morir

    /* ───────────── MOVIMIENTO / PICKUP ────── */
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    private CharacterController controller;
    private float moveInput;
    private float rotationInput;
    private Rigidbody rb;

    private Vector3 moveDirection = Vector3.zero;
    public float gravity = 9.81f;

    private GrabbableObject currentGrabbableObject;
    public Transform grabPoint;
    private readonly List<GrabbableObject> grabbedObjects = new();

    public GameObject smokeEffectPrefab;
    public AudioClip pickupSound;
    public AudioClip dropSound;
    private AudioSource audioSource;

    /* ───────────── LIFE‑CYCLE ─────────────── */
    void Start()
    {
        /* Salud */
        currentHealth = maxHealth;

        /* Componentes */
        controller = GetComponent<CharacterController>();
        if (!controller) Debug.LogError("Falta CharacterController");

        rb = GetComponent<Rigidbody>();
        if (!rb) Debug.LogError("Falta Rigidbody");

        audioSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
        if (!audioSource) Debug.LogError("Falta AudioSource");
    }

    void Update()
    {
        /* Movimiento */
        moveInput = Input.GetAxis("Vertical");
        rotationInput = Input.GetAxis("Horizontal");

        Vector3 forward = transform.forward * moveInput * moveSpeed;
        moveDirection = new Vector3(forward.x, moveDirection.y, forward.z);

        if (!controller.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        else
            moveDirection.y = -1f;

        controller.Move(moveDirection * Time.deltaTime);

        float rotation = rotationInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotation, 0f);

        /* Grabar / soltar */
        if (Input.GetKeyDown(KeyCode.Space))
            HandleGrabLogic();
    }

    private void FixedUpdate()
    {
        Vector3 forwardVelocity = transform.forward * moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(forwardVelocity.x, rb.linearVelocity.y, forwardVelocity.z);  // linearVelocity → velocity

        float rotation = rotationInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }

    /* ───────────── SALUD ───────────── */
    public void TakeDamage(int amount = 1)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
        else Debug.Log($"Player recibió daño. Vida restante: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        Debug.Log("Player muerto 🚀");

        if (deathEffectPrefab)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Deshabilitar controles para evitar más acciones mientras muere
        enabled = false;

        // Ejemplo simple: recargar la escena tras 2 s
        StartCoroutine(ReloadSceneAfterDelay(2f));
    }

    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /* ───────────── GRAB / DROP (sin cambios en la lógica principal) ───────────── */
    private void HandleGrabLogic()
    {
        if (currentGrabbableObject != null && !grabbedObjects.Contains(currentGrabbableObject))
        {
            if (grabbedObjects.Count == 0)
            {
                AgarrarObjeto(currentGrabbableObject);
            }
            else if (grabbedObjects.Count == 1)
            {
                if (grabbedObjects[0].CompareTag("Plato"))
                    AgarrarObjeto(currentGrabbableObject);
                else
                {
                    SoltarObjeto(grabbedObjects[0]);
                    grabbedObjects.Clear();
                    AgarrarObjeto(currentGrabbableObject);
                }
            }
            else if (grabbedObjects.Count == 2)
            {
                if (grabbedObjects[0].CompareTag("Plato"))
                {
                    GrabbableObject top = grabbedObjects[1];
                    grabbedObjects.RemoveAt(1);
                    SoltarObjeto(top);
                    AgarrarObjeto(currentGrabbableObject);
                }
                else
                {
                    foreach (var obj in grabbedObjects)
                        SoltarObjeto(obj);
                    grabbedObjects.Clear();
                    AgarrarObjeto(currentGrabbableObject);
                }
            }
            currentGrabbableObject = null;
        }
        else if (grabbedObjects.Count > 0)
        {
            GrabbableObject last = grabbedObjects[^1];
            grabbedObjects.RemoveAt(grabbedObjects.Count - 1);
            SoltarObjeto(last);
        }
    }

    /* ───────────── MÉTODOS PICKUP / DROP (sin cambios relevantes) ───────────── */
    private void AgarrarObjeto(GrabbableObject obj)
    {
        float totalHeight = 0f;
        foreach (var o in grabbedObjects)
        {
            if (o.TryGetComponent(out Collider col))
                totalHeight += col.bounds.size.y + 0.01f;
        }

        grabbedObjects.Add(obj);

        obj.transform.SetParent(grabPoint);
        obj.transform.localPosition = new Vector3(0f, totalHeight, 0f);
        obj.transform.localRotation = Quaternion.identity;

        if (obj.TryGetComponent(out Rigidbody objRb))
            objRb.isKinematic = true;
        if (obj.TryGetComponent(out Collider colObj))
            colObj.enabled = false;

        if (smokeEffectPrefab)
            Destroy(Instantiate(smokeEffectPrefab, obj.transform.position, Quaternion.identity), 2f);

        if (obj.TryGetComponent(out JuicyObject juicy))
            juicy.PlayPickupFeedback(pickupSound);

        Debug.Log("Agarrado: " + obj.name);
    }

    private void SoltarObjeto(GrabbableObject obj)
    {
        obj.transform.SetParent(null);
        obj.transform.position = grabPoint.position + transform.forward * 0.3f + Vector3.down * 0.1f;

        if (obj.TryGetComponent(out Rigidbody objRb))
            objRb.isKinematic = false;

        if (obj.TryGetComponent(out Collider col))
        {
            col.enabled = false;
            StartCoroutine(ReenableCollider(col));
        }

        audioSource?.PlayOneShot(dropSound);
        Debug.Log("Soltado: " + obj.name);
    }

    /* ───────────── HELPERS ───────────── */
    public void SetGrabbableObject(GrabbableObject g) => currentGrabbableObject = g;
    public void ClearGrabbableObject() => currentGrabbableObject = null;
    public bool HasObject(GrabbableObject obj) => grabbedObjects.Contains(obj);

    private IEnumerator ReenableCollider(Collider col)
    {
        yield return new WaitForSeconds(0.1f);
        col.enabled = true;
    }
}
