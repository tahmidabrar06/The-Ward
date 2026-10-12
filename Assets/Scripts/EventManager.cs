using TheWard.Combat;
using TheWard.Audio;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private bool waveDefenceStarted = false;
    private bool waveDefenceCompleted;

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
    private int currentWave = 0;
    [SerializeField]
    private float waveSpawnInterval, waveSpawnTimer, waveDefCountdown;
	private float uiUpdateTimer = 0f;

	private int startingWave;


    void Start()
    {
		startingWave = currentWave;
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
				uiUpdateTimer -= Time.deltaTime;
                if (uiUpdateTimer <= 0f)
                {
                    if (TallyMarker.Instance != null)
                    {
                        TallyMarker.Instance.UpdateZombieCount(GetAliveZombieCount());
                    }
                    uiUpdateTimer = 0.5f; 
                }
            }
            SpawnWaves();
        }
		else WaveDefConditionCheck();
    }

    public void StartWaveDefence()
    {
        if (waveDefenceStarted || waveDefenceCompleted) return;
        waveDefenceStarted = true;
        GameAudio.SetWaveDefense(true);
        ClearZombieContainer();
		
		TutorialManager.Instance.ShowBigPrompt($"Defend against {numberOfWaves - startingWave} zombie waves");		
		TallyMarker.Instance.ShowHUD();
    }

    public void SpawnWaves()
    {
        if (waveDefCountdown <= waveSpawnTimer && currentWave < numberOfWaves)
        {
			if(currentWave == startingWave+1) TutorialManager.Instance.RemoveBigPrompt();
			currentWave++;
            SpawnZombie(baseZombieAmount);
			TallyMarker.Instance.AddTally();
            waveSpawnTimer -= waveSpawnInterval;
        }
        if (currentWave == numberOfWaves && GetAliveZombieCount() == 0)
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
		TutorialManager.Instance.HideBigPrompt();
		TutorialManager.Instance.ShowPrompt("Get in the elevator");
		TallyMarker.Instance.HideHUD();

        if (waveDefenceCompleted) return;
        waveDefenceCompleted = true;
        waveDefenceStarted = false;
        GameAudio.SetWaveDefense(false);
        //uiManager.ShowVictoryScreen(); //Victory screen not implemented yet, substituting with respawnScreen()
        //uiManager.ShowRespawnScreen();
        elevatorTrig.victory = true; //needs to show victory screen instead of insta loading floor2
    }
    private void OnDisable()
    {
        if (waveDefenceStarted) GameAudio.SetWaveDefense(false);
    }

    private void OnEnable()
    {
        if (waveDefenceStarted) GameAudio.SetWaveDefense(true);
    }
}