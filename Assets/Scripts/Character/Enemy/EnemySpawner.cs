using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 적 게임 오브젝트를 주기적으로 생성
public class EnemySpawner : MonoBehaviour
{
    PlayerInput playerInput;
    // 현재 생성된 적들
    private readonly List<Enemy> enemies = new List<Enemy>();

    // 생성할 적들
    public Enemy[] enemyPrefabs;

    /*
    // 생성할 적의 최소 최대 데미지 
    public float damageMax = 40f;
    public float damageMin = 10f;

    // 생성할 적의 최소 최대 체력
    public float healthMax = 200f;
    public float healthMin = 50f;

    // 최대 최소 이동속도
    public float speedMax = 6f;
    public float speedMin = 1f;

    */
    // 생성 위치
    public Transform[] spawnPoints;

    // 다음 적 생성까지 시간
    private float nextWaveTime = 10f;
    // 현재 Wave 시간을 담아둘 변수
    private float currentWaveTime = 0f;

    // 현재 웨이브 단계
    private int wave;

    // 실행중인 코루틴
    List<Coroutine> coroutines = new List<Coroutine>();

    private void Start()
    {
        wave = 1;
        currentWaveTime = nextWaveTime;
        playerInput = FindObjectOfType<PlayerInput>();
    }

    private void Update()
    {
        // 게임매니저 인스턴스가 없거나 게임오버 상태면 아무것도 하지 않음
        if (GameManager.Instance != null && GameManager.Instance.isGameover)
            return;

        UIManager.Instance.UpdataRemaingEnemiesText(enemies.Count);

        // wave가 시작됬는데 필드에 적이 없다면 적 생성
        if (playerInput.StartWave && enemies.Count == 0)
        {
            UIManager.Instance.ShowEventMassge();

            // 웨이브 생성
            CoroutineManager.Instance.Run("SpwanWave", SpwanWave()); 
        }  
    }

    private IEnumerator SpwanWave()
    {
        yield return new WaitForSeconds(2.0f);

        // 현재 라운드 2배 만큼의 적 생성
        var spwanCount = Mathf.RoundToInt(wave *1.5f);

        for (int i = 0; i < spwanCount; i++)
        {
            // 적의 파위 초기화
            var enemyIntensity = wave * 0.5f;

            CreateEnemy(enemyIntensity);
        }

        UIManager.Instance.UpdateWaveText(wave, true);
        wave++;
    }

    private void CreateEnemy(float intensity) // 적 생성
    {
        /*
        // 체력, 데미지, 스피드, 생성위치 생성
        var health = Mathf.Lerp(healthMin, healthMax, intensity));
        var damage = Mathf.Lerp(damageMin, damageMax, intensity);
        var speed = Mathf.Lerp(speedMin, speedMax, intensity);
        var spwanPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        */

        var enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        // 체력, 데미지, 스피드, 생성위치 생성
        var health = enemy.startingHealth * intensity;
        var damage = enemy.damage * intensity;
        var patrolSpeed = enemy.patrolSpeed * intensity;
        var runSpeed = enemy.runSpeed * intensity;

        var spwanPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // enemy 프리팹 생성
        var enemyPrefab = Instantiate(enemy, spwanPoint.position, spwanPoint.rotation);

        Debug.Log("체력"+ enemy.health );
        Debug.Log("데미지" + enemy.damage);
        Debug.Log("정찰속도" + enemy.patrolSpeed);
        Debug.Log("달리기속도" + enemy.runSpeed);
        // enemy 능력치 초기화
        enemyPrefab.Setup(health, damage, runSpeed, patrolSpeed);

        // List에 추가(현재 씬에 있는 enemy)
        enemies.Add(enemyPrefab);

        // enemy가 죽었을떄 이벤트 추가
        enemyPrefab.OnDeath += () => enemies.Remove(enemyPrefab);
        enemyPrefab.OnDeath += () => Destroy(enemyPrefab.gameObject, 8f);
        enemyPrefab.OnDeath += () => GameManager.Instance.AddGold(enemy.dropGold);
    }
}