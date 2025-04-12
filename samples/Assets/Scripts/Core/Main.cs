using UnityEngine;
using Samples;
using System.Threading.Tasks;
using System.IO;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum GameState
{
    Booting,
    MainMenu,
    Settings,
    InGame,
    Paused,
    Results
}

public enum UIEventKey { OpenPauseMenu, BackToMainMenu }

public class Main : MonoBehaviour
{
    public static Main Instance;
    public Profile ProfileCache => Instance.profileCache;
    public GameState State => currentState;

    public static readonly Dictionary<UIEventKey, HashSet<GameState>> AllowedStates = new()
    {
        { UIEventKey.OpenPauseMenu, new HashSet<GameState> { GameState.InGame, GameState.Paused } },
        { UIEventKey.BackToMainMenu, new HashSet<GameState> { GameState.Results, GameState.Paused } },
    };

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private Timer roundTimer;
    [SerializeField] private GameRound[] gameRounds;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int mobDefeatedCount;
    [SerializeField] private BaseCameraComponent activeCamera;

    private GameState currentState;
    private Profile profileCache;
    private GameRound activeGameRound;
    private int roundIndex = 0;
    private bool roundHasStarted = false;
    private CharactorController playerController;
    private MobController[] mobControllers;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private async void Start()
    {
        SetState(GameState.MainMenu);

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

        gameUIController.Init(playerInput);
        gameUIController.OnStartButtonPressed = StartRound;
        gameUIController.OnNextRoundButtonPressed = SetUpNextRound;
        gameUIController.OnBackToMainMenu = () =>
        {
            roundHasStarted = false;
            Destroy(playerController.gameObject);
            ResetRound();
        };
        gameUIController.OnDebugFinishRoundButtonPressed = DebugFinishRound;
        gameUIController.OnDebugFinishGameButtonPressed = DebugFinishGame;
        roundHasStarted = false;
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
    }

    public bool IsEventAllowed(UIEventKey eventKey)
    {
        var state = Main.Instance.State;
        return AllowedStates.TryGetValue(eventKey, out var allowedStates) &&
               allowedStates.Contains(state);
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
        SetState(GameState.InGame);

        activeGameRound = gameRounds[roundIndex];

        roundTimer.ResetTimeText(activeGameRound.RoundMaxTime);

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
                playerController.Initialise(activeCamera, playerInput);
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
                mobControllers[i].SetNeighbors(mobControllers, i);
            }

        }

        await Task.Delay(2 * MathDefines.MILLISECOND_MULTIPLIER);

        async void OnTimerComplete()
        {
            gameUIController.HideHUD();

            await Task.Delay(1 * (MathDefines.MILLISECOND_MULTIPLIER / 2));

            gameUIController.GoToResultPanel(UIDefines.ResultPanelState.Time_Up, roundIndex + 1);

            roundHasStarted = false;
        }

        roundTimer.StartTimer(activeGameRound.RoundMaxTime, null, OnTimerComplete);
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
        roundTimer.StopTimer();
        roundTimer.ResetTimeText(activeGameRound.RoundMaxTime);
        mobDefeatedCount = 0;

        for (int i = 0; i < mobControllers.Length; i++)
        {
            Destroy(mobControllers[i].gameObject);
        }
    }

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

        roundTimer.StopTimer();

        var result = await UpdateProfileTask.Execute(1, (int)currentXp, roundIndex + 1);

        if (result)
        {
            var getProfileResult = await GetProfileTask.Execute(1);

            if (getProfileResult != null)
            {
                profileCache = getProfileResult;
            }

            await Task.Delay(1 * MathDefines.MILLISECOND_MULTIPLIER);

            async void GoToResultScreen()
            {
                gameUIController.HideHUD();

                await Task.Delay(1 * (MathDefines.MILLISECOND_MULTIPLIER / 2));

                var resultPanelState = roundIndex == gameRounds.Length - 1 ? UIDefines.ResultPanelState.Game_Complete : UIDefines.ResultPanelState.Round_Complete;
                gameUIController.GoToResultPanel(resultPanelState, roundIndex + 1);

                roundHasStarted = false;
            }

            gameUIController.AnimateXpMeter(earnedXp, GoToResultScreen);
        }
    }
}
