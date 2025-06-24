using UnityEngine;
using System.Collections.Generic;

public class TrashCan : MonoBehaviour
{
    public int maxObjectsBeforeReset = 4;
    private int destroyedCount = 0;

    private List<GrabbableObject> trashedObjects = new List<GrabbableObject>();

    private void OnTriggerEnter(Collider other)
    {
        GrabbableObject obj = other.GetComponent<GrabbableObject>();
        if (obj != null)
        {
            // Guardamos en la lista para resucitar luego
            if (!trashedObjects.Contains(obj))
            {
                trashedObjects.Add(obj);
                obj.gameObject.SetActive(false); // lo hacemos "desaparecer"
                destroyedCount++;
                Debug.Log($"Objeto arrojado al basurero: {obj.name}");
            }

            if (destroyedCount >= maxObjectsBeforeReset)
            {
                ResetObjects();
            }
        }
    }

    private void ResetObjects()
    {
        Debug.Log("Restaurando objetos desde el basurero...");

        foreach (var obj in trashedObjects)
        {
            obj.transform.position = obj.originalPosition;
            obj.transform.rotation = obj.originalRotation;
            obj.transform.SetParent(null); // evitar quedarse en la mano

            obj.gameObject.SetActive(true);

            // Reactivar físicas y collider
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            var col = obj.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        trashedObjects.Clear();
        destroyedCount = 0;
    }
}
