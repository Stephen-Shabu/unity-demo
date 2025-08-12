using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using static UIDefines;
using System.Collections;

public class GameUIController : MonoBehaviour
{
    public Action<Action>OnStartButtonPressed;
    public Action OnBackToMainMenu;
    public Action OnNextRoundButtonPressed;
    public Action OnSettingsButtonPressed;
    public Action OnDebugFinishRoundButtonPressed;
    public Action OnDebugFinishGameButtonPressed;

    [SerializeField] private GameUIView gameUIView;
    [SerializeField] private XpMeterController xpController;
    [SerializeField] private AudioSource gameUIAudioSource;
    [SerializeField] private AnimationCurve panelTransistionCurve;

    private IEnumerator IEFadeBackground;
    private PlayerInput playerInput;
    private Vector2 targetPosition;
    private Vector2 targetHUDPosition;
    private Vector2 targetPausePanelPosition;
    private bool isPaused;

    public void Init(PlayerInput input)
    {
        playerInput = input;

        playerInput.actions["Pause"].performed -= OnPause;
        playerInput.actions["Pause"].performed += OnPause;

        gameUIView.PanelBackgroundCanvasGroup.alpha = 1f;
        targetPosition = gameUIView.PanelRoot.anchoredPosition;

        GameEventsEmitter.OnEvent(EventType.ChangeState, OnEventStateChanged);
        GameEventsEmitter.OnEvent(EventType.ChangeWeapon, OnWeaponChanged);

        gameUIView.NextRoundButton.onClick.AddListener(() =>
        {
            gameUIAudioSource.PlayOneShot(gameUIView.NextRoundSFX, 1);
            OnNextRoundButtonPressed?.Invoke();
            GoToGamePanel(false);
        });

        gameUIView.StartButton.onClick.AddListener(() =>
        {
            gameUIAudioSource.PlayOneShot(gameUIView.ButtonConfirmSFX, 1);
            OnStartButtonPressed?.Invoke(() => { GoToGamePanel(true); }); 
        });
        
        gameUIView.SettingsButton.onClick.AddListener(() => 
        {
            GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.Settings });
            gameUIAudioSource.PlayOneShot(gameUIView.ButtonConfirmSFX, 1);
            OnSettingsButtonPressed?.Invoke();
            NavigateToPanel(-1);
        });

        gameUIView.ExitGameButton.onClick.AddListener(()=> 
        {
            Application.Quit();
        });

        gameUIView.SettingsBackButton.onClick.AddListener(() =>
        {
            GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.MainMenu });
            gameUIAudioSource.PlayOneShot(gameUIView.ButtonConfirmSFX, 1);
            NavigateToPanel(0);
        });

        gameUIView.GameCompleteExitButton.onClick.AddListener(()=>ReturnToHomePanel());

        gameUIView.PauseMenuExitButton.onClick.AddListener(() =>
        {
            isPaused = false;
            if (IEFadeBackground != null) StopCoroutine(IEFadeBackground);
            IEFadeBackground = FadeBackground(1, 0, 0.5f);
            StartCoroutine(IEFadeBackground);

            HidePausePanel(true);
            ReturnToHomePanel(()=> gameUIView.StartButton.Select());
        });

        gameUIView.ResultExitButton.onClick.AddListener(() => 
        {
            ReturnToHomePanel();
        });


        gameUIView.DebugFinishRoundButton.onClick.AddListener(() =>
        {
            OnDebugFinishRoundButtonPressed?.Invoke();
        });

        gameUIView.DebugFinishGameButton.onClick.AddListener(() =>
        {
            OnDebugFinishGameButtonPressed?.Invoke();
        });

        HidePausePanel(true);
    }

    public void InitialseHUD()
    {
        xpController.Initialise();
        HideHUD();
    }

    private void OnEventStateChanged(EventData e)
    {
        StateEventData stateData;

        if (e is StateEventData value)
        {
            stateData = value;

            gameUIView.StartButton.interactable = stateData.State.Equals(GameState.MainMenu);
            gameUIView.SettingsButton.interactable = stateData.State.Equals(GameState.MainMenu);
            gameUIView.ExitGameButton.interactable = stateData.State.Equals(GameState.MainMenu);

            gameUIView.SettingsBackButton.interactable = stateData.State.Equals(GameState.Settings);

            gameUIView.PauseMenuResumeButton.interactable = stateData.State.Equals(GameState.Paused);
            gameUIView.PauseMenuExitButton.interactable = stateData.State.Equals(GameState.Paused);
            gameUIView.PauseMenuExitAppButton.interactable = stateData.State.Equals(GameState.Paused);

            gameUIView.ResultExitButton.interactable = stateData.State.Equals(GameState.Results);
            gameUIView.GameCompleteExitButton.interactable = stateData.State.Equals(GameState.Results);
            gameUIView.NextRoundButton.interactable = stateData.State.Equals(GameState.Results);
            gameUIView.RestartRoundButton.interactable = stateData.State.Equals(GameState.Results);
        }
    }

    private void OnWeaponChanged(EventData e)
    {
        WeaponChangeEventData data;

        if (e is WeaponChangeEventData value)
        {
            data = value;

            switch (data.Name)
            {
                case WeaponName.BLASTER:
                    gameUIView.BlasterUI.color = Color.green;
                    gameUIView.WaveBeamUI.color = new Vector4(Color.grey.r, Color.grey.g, Color.grey.b, 0.25f);
                    break;

                case WeaponName.WAVE_BEAM:
                    gameUIView.BlasterUI.color = new Vector4(Color.grey.r, Color.grey.g, Color.grey.b, 0.25f);
                    gameUIView.WaveBeamUI.color = Color.green;
                    break;
            }
        }
    }

    private void GoToGamePanel(bool canFadeBackground)
    {
        NavigateToPanel(1);

        if (canFadeBackground)
        {
            if (IEFadeBackground != null) StopCoroutine(IEFadeBackground);

            IEFadeBackground = FadeBackground(1f, 0f, 1f);

            StartCoroutine(IEFadeBackground);
        }
    }

    private void ReturnToHomePanel(Action onComplete = null)
    {
        GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.MainMenu });

        SetStartButtonText();
        OnBackToMainMenu?.Invoke();

        if (IEFadeBackground != null) StopCoroutine(IEFadeBackground);
        IEFadeBackground = FadeBackground(0, 1f, 1f);
        StartCoroutine(IEFadeBackground);

        NavigateToPanel(0, false, () =>
        {
            onComplete?.Invoke();
        });
    }

    public void GoToResultPanel(ResultPanelState state, int roundIndex)
    {
        GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.Results });

        switch (state)
        {
            case ResultPanelState.Round_Complete:
                gameUIAudioSource.PlayOneShot(gameUIView.ResultScreenSFX, 1);
                gameUIView.NextRoundButton.Select();
                gameUIView.RoundCompleteText.SetText(string.Format(UIDefines.ROUND_COMPLETE_TEXT, roundIndex));
                break;
            case ResultPanelState.Time_Up:
            case ResultPanelState.You_Died:
                gameUIAudioSource.PlayOneShot(gameUIView.GameOverSFX, 1);
                var gameOverBodyText = state.Equals(ResultPanelState.Time_Up) ? UIDefines.TIME_UP_TEXT : UIDefines.YOU_DIED_TEXT;
                gameUIView.GameOverText.SetText(gameOverBodyText);
                gameUIView.RestartRoundButton.Select();
                break;
            case ResultPanelState.Game_Complete:
                gameUIAudioSource.PlayOneShot(gameUIView.GameCompleteScreenSFX, 1);
                gameUIView.GameCompleteExitButton.Select();
                break;
        }

        gameUIView.GameCompletePanel.SetActive(state.Equals(ResultPanelState.Game_Complete));
        gameUIView.RoundCompletePanel.SetActive(state.Equals(ResultPanelState.Round_Complete));
        gameUIView.GameOverPanel.SetActive(state.Equals(ResultPanelState.Time_Up) || state.Equals(ResultPanelState.You_Died));

        NavigateToPanel(2, false);
    }

    public void AnimateXpMeter(float amount, Action onComplete)
    {
        ShowHUD();
        xpController.AddExperience(amount, onComplete);
    }

    private void ShowHUD()
    {
        var hudHeight = xpController.XpMeterView.Root.rect.height;
        targetHUDPosition = new Vector2(0, hudHeight);
        MovePanel(xpController.XpMeterView.Root, targetHUDPosition, 1f);
    }

    public void HideHUD()
    {
        var hudHeight = xpController.XpMeterView.Root.rect.height;
        targetHUDPosition = new Vector2(0, -hudHeight);
        MovePanel(xpController.XpMeterView.Root, targetHUDPosition, 1f);
    }

    private void ShowPausePanel()
    {
        gameUIAudioSource.PlayOneShot(gameUIView.PauseMenuInSFX, 1);
        targetPausePanelPosition = new Vector2(0, 0);

        MovePanel(gameUIView.PausePanel, targetPausePanelPosition, 0.5f);
    }

    private void HidePausePanel(bool canSetImmediate = false)
    {
        var panelHeight = gameUIView.PanelRoot.rect.height;
        targetPausePanelPosition = new Vector2(0, -panelHeight);

        if (!canSetImmediate)
        {
            gameUIAudioSource.PlayOneShot(gameUIView.PauseMenuOutSFX, 1);
            MovePanel(gameUIView.PausePanel, targetPausePanelPosition, 0.5f);
        }
        else
        {
            gameUIView.PausePanel.anchoredPosition = targetPausePanelPosition;
        }
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        var isPressed = context.action.IsPressed();
        var playerActionMap = playerInput.actions.FindActionMap("Player");

        if(GameStateController.Instance.IsEventAllowed(UIEventKey.OpenPauseMenu))
        {
            if (isPressed && !isPaused)
            {
                GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.Paused });

                playerActionMap.Disable();
                gameUIView.PauseMenuResumeButton.Select();

                isPaused = true;

                if (IEFadeBackground != null) StopCoroutine(IEFadeBackground);
                IEFadeBackground = FadeBackground(0, 1, 0.5f);
                StartCoroutine(IEFadeBackground);

                ShowPausePanel();
            }
            else if (isPressed && isPaused)
            {
                GameEventsEmitter.EmitEvent(EventType.ChangeState, new StateEventData { State = GameState.InGame });

                playerActionMap.Enable();

                isPaused = false;

                if (IEFadeBackground != null) StopCoroutine(IEFadeBackground);
                IEFadeBackground = FadeBackground(1, 0, 0.5f);
                StartCoroutine(IEFadeBackground);

                HidePausePanel();
            }
        }
    }

    public void NavigateToPanel(int panelIndex, bool canSetImmediate = false, Action onComplete = null)
    {
        if (!canSetImmediate)
        {
            float canvasWidth = gameUIView.PanelRoot.rect.width;
            targetPosition = new Vector2(-canvasWidth * panelIndex, 0);

            MovePanel(gameUIView.PanelRoot, targetPosition, gameUIView.ButtonConfirmSFX.length, onComplete);
        }
        else
        {
            float canvasWidth = gameUIView.PanelRoot.rect.width;
            targetPosition = new Vector2(-canvasWidth * panelIndex, 0);
            gameUIView.PanelRoot.anchoredPosition = targetPosition;
            onComplete?.Invoke();
        }
    }

    public void SetStartButtonText()
    {
        var profile = Main.Instance.ProfileCache;
        var title = profile.LastCompletedLevel > 0 ? UIDefines.RESUME_TEXT : UIDefines.START_TEXT;
        gameUIView.StartButtonText.SetText(title);
    }

    private async void MovePanel(RectTransform panel, Vector2 target, float timeToReachTarget = 2.0f, Action onComplete = null)
    {
        float animTime = 0;
        var startPos = panel.anchoredPosition;

        while (Mathf.Abs(animTime - timeToReachTarget) > 0.01f)
        {
            animTime = Mathf.MoveTowards(animTime, timeToReachTarget, Time.deltaTime);
            var newPosition = Vector2.Lerp(startPos, target, panelTransistionCurve.Evaluate(animTime / timeToReachTarget));
            panel.anchoredPosition = newPosition;

            await Task.Yield();
        }

        onComplete?.Invoke();
    }

    private IEnumerator FadeBackground(float startAlpha, float targetAlpha, float timeToReachTarget = 2.0f)
    {
        float animTime = 0;

        while (Mathf.Abs(animTime - timeToReachTarget) > 0.01f)
        {
            animTime += Time.deltaTime;

            var newAlpha = Mathf.Lerp(startAlpha, targetAlpha, animTime / timeToReachTarget);
            gameUIView.PanelBackgroundCanvasGroup.alpha = newAlpha;

            yield return new WaitForEndOfFrame();
        }
    }

    private void OnDestroy()
    {
        if(playerInput != null) playerInput.actions["Pause"].performed -= OnPause;
    }
}
