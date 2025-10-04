using System.Linq;
using UnityEngine;

public class PowerUpShop : MonoBehaviour
{
    [SerializeField] private float spaceOffset = 3.0f;
    [SerializeField] private int powerUpCount = 3;
    [SerializeField] private GameObject[] powerUpPrefabs;

    private void Awake()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount)
            Debug.Log("Not enough power up prefabs added!");
    }

    void Start()
    {
        SpawnPowerUp();
    }

    void Update()
    {
        
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Count() < powerUpCount) return;
        
        for (int i = 0; i < powerUpCount; i++)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            Vector3 prefabPosition = transform.position + Vector3.up * i * spaceOffset;
            Instantiate(powerUpPrefabs[randomIndex], prefabPosition, Quaternion.identity, transform);
        }
    }
}
