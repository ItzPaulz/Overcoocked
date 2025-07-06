using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    public bool canGrab = false;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;

    private JuicyObject juicy;

    void Start()
    {
        juicy = GetComponent<JuicyObject>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canGrab = true;
            Debug.Log("Jugador entró en rango de: " + name);

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null && !player.HasObject(this))
            {
                player.SetGrabbableObject(this);
            }

            if (juicy != null)
                juicy.ShowHighlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canGrab = false;
            Debug.Log("Jugador salió de rango de: " + name);

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.ClearGrabbableObject();
            }

            if (juicy != null)
                juicy.HideHighlight();
        }
    }

}
