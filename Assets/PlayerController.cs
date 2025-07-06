using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
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
    private List<GrabbableObject> grabbedObjects = new List<GrabbableObject>();

    public GameObject smokeEffectPrefab;
    public AudioClip pickupSound;
    public AudioClip dropSound;
    private AudioSource audioSource;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!controller) Debug.LogError("Falta CharacterController");

        rb = GetComponent<Rigidbody>();
        if (!rb) Debug.LogError("Falta Rigidbody");

        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = GetComponentInChildren<AudioSource>();
            if (!audioSource)
                Debug.LogError("Falta AudioSource");
        }
    }

    void Update()
    {
        // Movimiento
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

        // Agarrar o soltar con tecla Espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentGrabbableObject != null && !grabbedObjects.Contains(currentGrabbableObject))
            {
                string newTag = currentGrabbableObject.tag;

                if (grabbedObjects.Count == 0)
                {
                    AgarrarObjeto(currentGrabbableObject);
                    currentGrabbableObject = null;
                }
                else if (grabbedObjects.Count == 1)
                {
                    string firstTag = grabbedObjects[0].tag;

                    if (firstTag == "Plato")
                    {
                        AgarrarObjeto(currentGrabbableObject);
                        currentGrabbableObject = null;
                    }
                    else
                    {
                        SoltarObjeto(grabbedObjects[0]);
                        grabbedObjects.Clear();

                        AgarrarObjeto(currentGrabbableObject);
                        currentGrabbableObject = null;
                    }
                }
                else if (grabbedObjects.Count == 2)
                {
                    string firstTag = grabbedObjects[0].tag;

                    if (firstTag == "Plato")
                    {
                        GrabbableObject topObject = grabbedObjects[1];
                        grabbedObjects.RemoveAt(1);
                        SoltarObjeto(topObject);

                        AgarrarObjeto(currentGrabbableObject);
                        currentGrabbableObject = null;
                    }
                    else
                    {
                        foreach (var obj in grabbedObjects)
                            SoltarObjeto(obj);
                        grabbedObjects.Clear();

                        AgarrarObjeto(currentGrabbableObject);
                        currentGrabbableObject = null;
                    }
                }
            }
            else if (grabbedObjects.Count > 0)
            {
                GrabbableObject last = grabbedObjects[grabbedObjects.Count - 1];
                grabbedObjects.RemoveAt(grabbedObjects.Count - 1);
                SoltarObjeto(last);
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 forwardVelocity = transform.forward * moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(forwardVelocity.x, rb.linearVelocity.y, forwardVelocity.z);

        float rotation = rotationInput * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turn = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }

    private void AgarrarObjeto(GrabbableObject obj)
    {
        float totalHeight = 0f;
        foreach (var o in grabbedObjects)
        {
            Collider col = o.GetComponent<Collider>();
            if (col != null) totalHeight += col.bounds.size.y + 0.01f;
        }

        grabbedObjects.Add(obj);

        obj.transform.SetParent(grabPoint);
        obj.transform.localPosition = new Vector3(0f, totalHeight, 0f);
        obj.transform.localRotation = Quaternion.identity;

        Rigidbody objRb = obj.GetComponent<Rigidbody>();
        if (objRb != null) objRb.isKinematic = true;

        Collider colObj = obj.GetComponent<Collider>();
        if (colObj != null) colObj.enabled = false;

        if (smokeEffectPrefab != null)
        {
            GameObject effect = Instantiate(smokeEffectPrefab, obj.transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        JuicyObject juicy = obj.GetComponent<JuicyObject>();
        if (juicy != null)
            juicy.PlayPickupFeedback(pickupSound);

        Debug.Log("Agarrado: " + obj.name);
    }

    private void SoltarObjeto(GrabbableObject obj)
    {
        obj.transform.SetParent(null);
        obj.transform.position = grabPoint.position + transform.forward * 0.3f + Vector3.down * 0.1f;

        Rigidbody objRb = obj.GetComponent<Rigidbody>();
        if (objRb != null) objRb.isKinematic = false;

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(ReenableCollider(col));
        }

        if (dropSound != null && audioSource != null)
            audioSource.PlayOneShot(dropSound);

        Debug.Log("Soltado: " + obj.name);
    }

    public void SetGrabbableObject(GrabbableObject grabbableObject)
    {
        currentGrabbableObject = grabbableObject;
        Debug.Log("Objeto disponible: " + grabbableObject.name);
    }

    public void ClearGrabbableObject()
    {
        currentGrabbableObject = null;
        Debug.Log("Saliste del trigger.");
    }

    public bool HasObject(GrabbableObject obj)
    {
        return grabbedObjects.Contains(obj);
    }

    private IEnumerator ReenableCollider(Collider col)
    {
        yield return new WaitForSeconds(0.1f);
        col.enabled = true;
    }
}
