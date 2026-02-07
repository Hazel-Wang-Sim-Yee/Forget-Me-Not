using UnityEngine;

public class ItemSpawnerScript : MonoBehaviour
{
    [SerializeField]
    public GameObject itemPrefab;
    [SerializeField]
    public Transform spawnPoint;

    void OnCollisionEnter(Collision collision)
    {
        SpawnItem();   
    }

    public void SpawnItem()
    {
        Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
