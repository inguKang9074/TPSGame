using UnityEngine;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] items; // 생성할 아이템
    public Transform playerTransform; // 생성 기준점
    
    private float lastSpawnTime; // 마지막 생성 시점
    public float maxDistance = 6f; // 생성 범위
    
    private float timeBetSpawn; // 생성주기

    public float timeBetSpawnMax = 10f; // 최대 생성 주기
    public float timeBetSpawnMin = 5f; // 최소 생성 주기

    private void Start()
    {
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = 0f;
    }

    private void Update()
    {
        if (playerTransform != null && Time.time >= lastSpawnTime + timeBetSpawn)
        {
            Spawn();
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        }
    }

    private void Spawn()
    {
        var spwanPosition = Utility.GetRandomPointOnNavMesh(playerTransform.position, maxDistance, NavMesh.AllAreas);

        spwanPosition += Vector3.up * 0.5f;

       // var item = Instantiate(items[Random.Range(0, items.Length)], spwanPosition, Quaternion.identity);

       // Destroy(item, 5f);
    }
}