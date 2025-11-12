using UnityEngine;
using Samples;
using System.Threading.Tasks;
using System.IO;
using System;
using UnityEngine.InputSystem;
using static UIDefines;

public class Main : MonoBehaviour
{
    public static Main Instance;
    public Profile ProfileCache => Instance.profileCache;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private HitStopController hitStopController;
    [SerializeField] private CombatDirector combatDirector;
    [SerializeField] private WeaponDatabase weaponDb;
    [SerializeField] private Timer roundTimer;
    [SerializeField] private GameRound[] gameRounds;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int mobDefeatedCount;
    [SerializeField] private BaseCameraComponent activeCamera;

    private Profile profileCache;
    private GameRound activeGameRound;
    private int roundIndex = 0;
    private bool roundHasStarted = false;
    private CharactorController playerController;
    private MobController[] mobControllers;
    private GameStateController gameStateController;
    private PlayerInput cachedPlayerInput;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        gameStateController = new GameStateController();
        gameStateController.HandleOnControlsChanged(cachedPlayerInput);
        roundTimer.Initialise();
        hitStopController.Initialise();
        weaponDb.Initialise();
    }

    private async void Start()
    {
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

        GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.MainMenu });
        GameEventsEmitter.OnEvent(EventType.EnemyDefeated, HandleMobDefeated);
        GameEventsEmitter.OnEvent(EventType.PlayerDefeated, HandlePlayerDefeated);
        GameEventsEmitter.OnEvent(EventType.HitRegistered, HandleHitRegistered);

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
        gameUIController.SetStartButtonText();
        gameUIController.InitialseHUD();
    }

    private void OnControlsChanged(PlayerInput input)
    {
        if(cachedPlayerInput == null)
            cachedPlayerInput = input;

        if (Instance != null)
        {
            gameStateController.HandleOnControlsChanged(input);
        }
    }

    private void DebugFinishRound()
    {
        for (int i = 0; i < activeGameRound.NumberOfEnemies; i++)
        {
            HandleMobDefeated(new GenericEventData { Type = EventType.EnemyDefeated});
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
        GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.InGame });

        activeGameRound = gameRounds[roundIndex];

        roundTimer.ResetTimeText(activeGameRound.RoundMaxTime);

        if (roundIndex < gameRounds.Length)
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
                combatDirector.Initialise(playerController.gameObject);
            }
            else
            {
                playerController.transform.position = spawnPoint.position;
            }

            for (int i = 0; i < activeGameRound.NumberOfEnemies; i++)
            {
                var enemy = await InstantiateAsync(activeGameRound.enemyType, points[i], Quaternion.identity);
                enemy[0].name = $"Enemy unit {i}";

                var enemyController = enemy[0].GetComponent<MobController>();

                enemyController.Initialize(playerController.transform);
                mobControllers[i] = enemyController;
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

        var playerActionMap = playerInput.actions.FindActionMap("Player");
        playerActionMap.Enable();

        //roundTimer.StartTimer(activeGameRound.RoundMaxTime, null, OnTimerComplete);
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
        roundHasStarted = false;
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

    private void HandleHitRegistered(EventData e)
    {
        HitRegisterEventData hitData;

        if (e is HitRegisterEventData value)
        {
            hitData = value;

            if (hitData.Owner.Equals(HitRegisterEventData.HitOwner.Mob))
            {
                Debug.Log("Enemy hit");
            }
            else
            {
                Debug.Log("Player hit");
            }

        }
    }

    private void HandleMobDefeated(EventData e)
    {
        mobDefeatedCount++;

        if (mobDefeatedCount == activeGameRound.NumberOfEnemies)
        {
            CompleteRound();
        }
    }

    private async void HandlePlayerDefeated(EventData e)
    {
        await Task.Delay(1 * (MathDefines.MILLISECOND_MULTIPLIER / 2));

        gameUIController.GoToResultPanel(ResultPanelState.You_Died, roundIndex + 1);
        var playerActionMap = playerInput.actions.FindActionMap("Player");
        playerActionMap.Disable();
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

                var playerActionMap = playerInput.actions.FindActionMap("Player");
                playerActionMap.Disable();
            }

            gameUIController.AnimateXpMeter(earnedXp, GoToResultScreen);
        }
    }
}
