using TheWard.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    private bool waveDefenceStarted = false;

    [SerializeField]
    private float durationOfWaveDef;
    [SerializeField]
    private int numberOfWaves;
    [SerializeField]
    private float difficultyMultiplier, waveScaleMultiplier; //wave scale multi scales compundly
    [SerializeField]
    private int baseZombieAmount;
    private GameObject zombieSpawn;
    private GameObject zombieContainer;
    private GameObject waveDefTrigger;
    public GameObject zombiePrefab;
    private GameObject player;

    [Header("Live Values For Testing")]
    [SerializeField]
    private int currentWave = 1;
    [SerializeField]
    private float waveSpawnInterval, waveSpawnTimer, waveDefCountdown;


    void Start()
    {
        waveSpawnInterval = durationOfWaveDef / numberOfWaves;
        waveDefCountdown = durationOfWaveDef;
        waveSpawnTimer = durationOfWaveDef;

        player = GameObject.FindGameObjectWithTag("Player");
        zombieSpawn = GameObject.Find("ZombieSpawn");
        zombieContainer = GameObject.Find("ZombieContainer");
    }

    void Update()
    {
        if (waveDefenceStarted)
        {
            if (waveDefCountdown > 0)
            {
                waveDefCountdown -= Time.deltaTime;
            }
            if(player.GetComponent<Health>().CurrentHealth <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            SpawnWaves();
        }
    }

    public void StartWaveDefence()
    {
        waveDefenceStarted = true;
        ClearZombieContainer();
    }

    public void SpawnWaves()
    {
        if (waveDefCountdown <= waveSpawnTimer && currentWave <= numberOfWaves)
        {
            SpawnZombie(baseZombieAmount);
            waveSpawnTimer -= waveSpawnInterval;
            currentWave += 1;
        }
    }

    public void SpawnZombie(int amount)
    {
        int scaledByDifficultyAmount = Mathf.RoundToInt(amount * difficultyMultiplier * Mathf.Pow(waveScaleMultiplier, currentWave - 1));

        for (int i = 0; i < scaledByDifficultyAmount; i++)
        {
            GameObject zombie = Instantiate(zombiePrefab,zombieSpawn.transform.position,zombieSpawn.transform.rotation,zombieContainer.transform);
            zombie.GetComponent<Zombie>().SetZombieState(Zombie.ZombieState.Detected);
        }
    }
    void ClearZombieContainer()
    {
        Zombie[] zombies = zombieContainer.GetComponentsInChildren<Zombie>();
        foreach (Zombie zombie in zombies)
        {
            if (zombie.isDead)
            {
                Destroy(zombie.gameObject);
            }
            else
            {
                zombie.SetZombieState(Zombie.ZombieState.Detected);
            }
        }
    }
}