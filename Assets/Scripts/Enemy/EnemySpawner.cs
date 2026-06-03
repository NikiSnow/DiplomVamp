using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] float SpawnDelay = 1;
    [SerializeField] GameObject Player;
    [SerializeField] Player ThePlayer;

    [Header("Timer")]
    [SerializeField] TMP_Text TimerText;
    public float elapsedTime = 0f;
    private bool isRunning = true;

    [SerializeField] GameObject ChestPrefab;
    [SerializeField] float ChestSpawnDelay = 15;
    [SerializeField] Casino CasinoScr;

    [SerializeField] GameObject EnemiesObj;
    [SerializeField] GameObject ChestsObj;

    [SerializeField] int AddHp = 5;
    [SerializeField] float AddDmg = 0.5f;
    [SerializeField] float AddSpeed = 0.05f;

    private void Start()
    {
        StartCoroutine(EnemySpawn());
        StartCoroutine(ChestSpawn());
    }

    private void Update()
    {
        if (!isRunning) return;

        // Увеличиваем время
        elapsedTime += Time.deltaTime;

        // Обновляем текст
        UpdateTimerDisplay();

    }

    private void UpdateTimerDisplay()
    {
        // Получаем минуты и секунды
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);

        // Форматируем в 00:00
        TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    IEnumerator EnemySpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(SpawnDelay);
            SpawnEnemy();
        }
    }
    IEnumerator ChestSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(ChestSpawnDelay);
            SpawnChest();
        }
    }
    public void SpawnChest()
    {
        Vector2 poses = GetRandomPoint();
        GameObject CurrChest = Instantiate(ChestPrefab);
        CurrChest.transform.SetParent(ChestsObj.transform);
        CurrChest.transform.position = new Vector3(Player.transform.position.x + poses.x + 20, Player.transform.position.y + poses.y + 10, 0);
        CurrChest.transform.rotation = Quaternion.identity;
        Chest CurrChestScr = CurrChest.GetComponent<Chest>();
        CurrChestScr.CasinoScr = CasinoScr;

    }

    public void SpawnEnemy()
    {
        Vector2 poses = GetRandomPoint();
        GameObject CurrEnemy = Instantiate(EnemyPrefab);
        CurrEnemy.transform.SetParent(EnemiesObj.transform);
        CurrEnemy.transform.position = new Vector3(Player.transform.position.x + poses.x, Player.transform.position.y + poses.y, 0);
        Enemy CurrEnemyScr = CurrEnemy.GetComponent<Enemy>();
        CurrEnemyScr.target = Player.transform;
        CurrEnemyScr.ThePlayer = ThePlayer;
        int multip = (int)(elapsedTime / 10);
        CurrEnemyScr.AddHp(AddHp * multip);
        CurrEnemyScr.AddDmg(AddDmg * multip);
        CurrEnemyScr.AddSpeed(AddSpeed * multip);
        CurrEnemy.transform.rotation = Quaternion.identity;

    }

    public Vector2 GetRandomPoint()
    {
        // Случайно выбираем, какое условие будет выполняться
        bool satisfyXCondition = Random.value < 0.5f;
        bool isMinusX = Random.value < 0.5f;
        bool isMinusY = Random.value < 0.5f;

        float x;
        float y;

        float multX = isMinusX ? -1f : 1f;
        float multY = isMinusY ? -1f : 1f;

        if (satisfyXCondition)
        {
            // x > 600, y - полностью случайный от 0 до 1100
            x = 10f * multX;
            y = Random.Range(0f, 6f) * multY;
        }
        else
        {
            // y > 1100, x - полностью случайный от 0 до 600
            x = Random.Range(0f, 10f) * multX;
            y = 6f * multY;
        }

        return new Vector2(x, y);
    }

}
