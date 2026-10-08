using TheWard.Combat;
using UnityEngine;

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
    private UIManager uiManager;
    private TriggerElevator elevatorTrig;

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
        waveDefTrigger = GameObject.Find("WaveDefenseTrigger");
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        elevatorTrig = GameObject.Find("ElevatorOpenTrigger").GetComponent<TriggerElevator>();
    }

    void Update()
    {
        if (waveDefenceStarted)
        {
            if (waveDefCountdown > 0)
            {
                waveDefCountdown -= Time.deltaTime;
            }
            SpawnWaves();
        }
		else WaveDefConditionCheck(); //temporary, to be replaced with elvator door trigger
    }

    public void StartWaveDefence()
    {
        TutorialManager.Instance.ShowBigPrompt("Defend against zombie waves");
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
        if (waveDefCountdown <= 0 && GetAliveZombieCount() == 0) // Consider changing to ((waveDefCountdown <= 0 || (currentWave >= numberOfWaves)) && GetAliveZombieCount() == 0)
        {
            Victory();
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

    void WaveDefConditionCheck()
    {
        if (Vector3.Distance(player.transform.position, waveDefTrigger.transform.position) 
            <= waveDefTrigger.GetComponent<SphereCollider>().radius)
        {
            StartWaveDefence();
        }
    }

    public int GetAliveZombieCount()
    {
        int aliveCount = 0;

        Zombie[] zombies = zombieContainer.GetComponentsInChildren<Zombie>();

        foreach (Zombie zombie in zombies)
        {
            if (!zombie.isDead)
            {
                aliveCount++;
            }
        }
        return aliveCount;
    }

    public float GetCountDown()
    {
        return waveDefCountdown;
    }
    public string GetWave()
    {
        return currentWave.ToString()+"/"+ numberOfWaves;
    }

    public void Victory()
    {
        //uiManager.ShowVictoryScreen(); //Victory screen not implemented yet, substituting with respawnScreen()
        //uiManager.ShowRespawnScreen();
		TutorialManager.Instance.HideBigPrompt();
        TutorialManager.Instance.ShowPrompt("Enter the elevator");
        elevatorTrig.victory = true; //needs to show victory screen instead of insta loading floor2
    }
}