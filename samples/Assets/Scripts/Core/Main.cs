using UnityEngine;
using Samples;
using System.Threading.Tasks;
using System.IO;
using System;
using Unity.VisualScripting;


public class Main : MonoBehaviour
{
    public static Main Instance;
    public Profile ProfileCache => Instance.profileCache;

    private Profile profileCache;

    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private GameRound[] gameRounds;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int mobDefeatedCount;
    [SerializeField] private BaseCameraComponent activeCamera;

    [SerializeField] private GameRound activeGameRound;
    private int roundIndex = 0;
    private bool roundHasStarted = false;
    private CharactorController playerController;
    private MobController[] mobControllers;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private async void Start()
    {
        if (File.Exists(GameDefines.DATABASE_FILE_PATH))
        {
            File.Delete(GameDefines.DATABASE_FILE_PATH);
            Debug.Log("Database reset: Deleted old database file.");
        }

        if (!File.Exists(GameDefines.DATABASE_FILE_PATH))
        {
            var createDbResult = await CreateDatabaseTask.Execute();

            if (createDbResult)
            {
                var createProfileResult = await CreateProfileTask.Execute();

                if (createProfileResult != null)
                {
                    profileCache = createProfileResult;
                    roundIndex = profileCache.LastCompletedLevel;
                }
            }
        }
        else
        {
            var getProfileResult = await GetProfileTask.Execute(1);

            if (getProfileResult != null)
            {
                profileCache = getProfileResult;
                roundIndex = profileCache.LastCompletedLevel;
            }
        }

        gameUIController.Init();
        gameUIController.OnStartButtonPressed = StartRound;
        gameUIController.OnNextRoundButtonPressed = SetUpNextRound;
        gameUIController.OnDebugFinishRoundButtonPressed = DebugFinishRound;
        gameUIController.OnDebugFinishGameButtonPressed = DebugFinishGame;
        roundHasStarted = false;
    }

    private void DebugFinishRound()
    {
        for (int i = 0; i < activeGameRound.NumberOfEnemies; i++)
        {
            HandleMobDefeated();
        }
    }

    private void DebugFinishGame()
    {
        var lastEnemyCount = 0;

        for (int j = 0; j < gameRounds.Length; j++)
        {
            var round = gameRounds[j];

            for (int i = 0; i < round.NumberOfEnemies; i++)
            {
                mobDefeatedCount++;
            }

            if (mobDefeatedCount == round.NumberOfEnemies + lastEnemyCount)
            {
                lastEnemyCount = mobDefeatedCount;

                if (roundIndex < gameRounds.Length - 1)
                {
                    roundIndex = roundIndex + 1;
                }
            }
        }

        CompleteRound();
    }

    private async void StartRound(Action onComplete = null)
    {
        activeGameRound = gameRounds[roundIndex];

        if (roundIndex < gameRounds.Length - 1)
        {
            var args = new SpawnerArgs()
            {
                ObjectToSpawn = activeGameRound.enemyType,
                SpawnCount = activeGameRound.NumberOfEnemies,
                spawnRadius = 30f,
                minSpawnRadius = 5f,
                objectRadius = 3f
            };

            var points = NoisySpawner.GetSpawnPoints(args, spawnPoint.position);
            mobControllers = new MobController[activeGameRound.NumberOfEnemies];

            if (playerController == null)
            {
                var player = await InstantiateAsync(playerPrefab, spawnPoint.position, Quaternion.identity);
                playerController = player[0].GetComponent<CharactorController>();
                activeCamera.Initialise(playerController.transform);
                playerController.Initialise(activeCamera);
            }

            for (int i = 0; i < activeGameRound.NumberOfEnemies; i++)
            {
                var enemy = await InstantiateAsync(activeGameRound.enemyType, points[i], Quaternion.identity);
                var enemyController = enemy[0].GetComponent<MobController>();
                enemyController.Initialize(playerController.transform);
                enemyController.OnHealthReachedZero -= HandleMobDefeated;
                enemyController.OnHealthReachedZero += HandleMobDefeated;
                mobControllers[i] = enemyController;
            }

            for (int i = 0; i < activeGameRound.NumberOfEnemies; i++)
            {
                mobControllers[i].SetNeighbors(mobControllers);
            }

        }

        await Task.Delay(2 * MathDefines.MILLISECOND_MULTIPLIER);
        roundHasStarted = true;
        onComplete?.Invoke();
    }

    private void SetUpNextRound()
    {
        ResetRound();
        roundIndex = roundIndex + 1;
        StartRound();
    }

    private void ResetRound()
    {
        mobDefeatedCount = 0;

        for (int i = 0; i < mobControllers.Length ; i++)
        {
            Destroy(mobControllers[i].gameObject);
        }
    }

    private void Update()
    {
        gameUIController.UpdateGameUI();
    }

    private float flockingDistance = 0;
    private void FixedUpdate()
    {
        if (roundHasStarted)
        {
            playerController.UpdateController();

            foreach (var enemy in mobControllers)
            {
                enemy.UpdateController();
            }
        }
    }

    private void HandleMobDefeated()
    {
        mobDefeatedCount++;

        if (mobDefeatedCount == activeGameRound.NumberOfEnemies)
        {
            CompleteRound();
        }
    }

    private async void CompleteRound()
    {
        var currentXp = (float) profileCache.XpTotal;
        var earnedXp = RewardDefines.XP_PER_EMEMY * mobDefeatedCount;

        currentXp += earnedXp;

        var result = await UpdateProfileTask.Execute(1, (int)currentXp, roundIndex + 1);

        if (result)
        {
            await Task.Delay(1 * MathDefines.MILLISECOND_MULTIPLIER);

            async void GoToResultScreen()
            {
                gameUIController.HideHUD();

                await Task.Delay(1 * (MathDefines.MILLISECOND_MULTIPLIER / 2));

                var isGameComplete = roundIndex == gameRounds.Length - 1;
                gameUIController.SetResultPanel(isGameComplete, roundIndex + 1);

                gameUIController.NavigateToPanel(2);

                roundHasStarted = false;
            }

            gameUIController.AnimateXpMeter(earnedXp, GoToResultScreen);
        }
    }
}
