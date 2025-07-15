using UnityEngine;

public class SpawnEnemyScript : MonoBehaviour
{
     public GameObject balaPrefab;

    void Start()
    {

        InvokeRepeating(nameof(SpawnEnemy), 0f, 10f);
        
    }

    void SpawnEnemy()
    {

        Instantiate(balaPrefab, transform.position, transform.rotation);
    }
}
