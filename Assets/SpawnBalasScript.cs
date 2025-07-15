using UnityEngine;

public class SpawnBalasScript : MonoBehaviour
{
    public GameObject balaPrefab;   // Arrastra aquí tu prefab de bala

    void Start()
    {
        // Ejecuta SpawnBala inmediatamente y luego cada 10 s
        InvokeRepeating(nameof(SpawnBala), 0f, 10f);
    }

    void SpawnBala()
    {
        // Instancia la bala en la posición y rotación del objeto que lleva este script
        Instantiate(balaPrefab, transform.position, transform.rotation);
    }
}
