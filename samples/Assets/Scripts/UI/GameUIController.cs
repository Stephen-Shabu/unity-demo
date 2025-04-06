using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Threading;
using static UIDefines;

public class GameUIController : MonoBehaviour
{
    public Action<Action>OnStartButtonPressed;
    public Action OnBackToMainMenu;
    public Action OnNextRoundButtonPressed;
    public Action OnSettingsButtonPressed;
    public Action OnDebugFinishRoundButtonPressed;
    public Action OnDebugFinishGameButtonPressed;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameUIView gameUIView;
    [SerializeField] private XpMeterController xpController;
    [SerializeField] private AudioSource gameUIAudioSource;
    [SerializeField] private AnimationCurve panelTransistionCurve;

    private Vector2 targetPosition;
    private Vector2 targetHUDPosition;
    private Vector2 targetPausePanelPosition;
    private bool isPaused;

    public void Init()
    {
        gameUIView.PanelBackgroundCanvasGroup.alpha = 1f;
        targetPosition = gameUIView.PanelRoot.anchoredPosition;
        SetStartButtonText();
        xpController.Initialise();

        gameUIView.NextRoundButton.onClick.AddListener(() =>
        {
            gameUIView.NextRoundButton.interactable = false;
            gameUIAudioSource.PlayOneShot(gameUIView.NextRoundSFX, 1);
            OnNextRoundButtonPressed?.Invoke();
            GoToGamePanel(false);
        });

        gameUIView.StartButton.onClick.AddListener(() =>
        {
            gameUIAudioSource.PlayOneShot(gameUIView.ButtonConfirmSFX, 1);
            gameUIView.StartButton.interactable = false;
            gameUIView.SettingsButton.interactable = false;
            OnStartButtonPressed?.Invoke(() => { GoToGamePanel(true); }); 
        });
        
        gameUIView.SettingsButton.onClick.AddListener(() => 
        {
            gameUIAudioSource.PlayOneShot(gameUIView.ButtonConfirmSFX, 1);

            gameUIView.SettingsBackButton.interactable = true;
            gameUIView.SettingsButton.interactable = false;
            OnSettingsButtonPressed?.Invoke();
            NavigateToPanel(-1);
        });

        gameUIView.SettingsBackButton.onClick.AddListener(() =>
        {
            gameUIAudioSource.PlayOneShot(gameUIView.ButtonConfirmSFX, 1);
            gameUIView.SettingsBackButton.interactable = false;
            gameUIView.SettingsButton.interactable = true;
            NavigateToPanel(0);
        });

        gameUIView.GameCompleteExitButton.onClick.AddListener(() =>
        {
            gameUIView.GameCompleteExitButton.interactable = false;

            ReturnToHomePanel();
        });

        gameUIView.PauseMenuExitButton.onClick.AddListener(() =>
        {
            gameUIView.PauseMenuExitButton.interactable = false;

            ReturnToHomePanel();
        });

        gameUIView.ResultExitButton.onClick.AddListener(() => 
        {
            gameUIView.ResultExitButton.interactable = false;

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

        HideHUD();
        HidePausePanel(true);
    }

    private void GoToGamePanel(bool canFadeBackground)
    {
        NavigateToPanel(1); 
        
        if(canFadeBackground) FadeBackground(1, 0);
    }

    private void ReturnToHomePanel(Action onComplete = null)
    {
        gameUIView.StartButton.interactable = false;
        gameUIView.SettingsButton.interactable = false;
        SetStartButtonText();
        OnBackToMainMenu?.Invoke();
        FadeBackground(0, 1, 1f);
        NavigateToPanel(0, false, () =>
        {
            onComplete?.Invoke();
            gameUIView.StartButton.interactable = true;
            gameUIView.SettingsButton.interactable = true;
        });
    }

    public void GoToResultPanel(ResultPanelState state, int roundIndex)
    {
        if (state.Equals(ResultPanelState.Round_Complete)) gameUIAudioSource.PlayOneShot(gameUIView.ResultScreenSFX, 1);
        if (state.Equals(ResultPanelState.Time_Up) || state.Equals(ResultPanelState.You_Died)) gameUIAudioSource.PlayOneShot(gameUIView.GameOverSFX, 1);

        gameUIView.GameCompletePanel.SetActive(state.Equals(ResultPanelState.Game_Complete));
        gameUIView.RoundCompletePanel.SetActive(state.Equals(ResultPanelState.Round_Complete));
        gameUIView.GameOverPanel.SetActive(state.Equals(ResultPanelState.Time_Up) || state.Equals(ResultPanelState.You_Died));

        var gameOverBodyText = state.Equals(ResultPanelState.Time_Up) ? UIDefines.TIME_UP_TEXT : UIDefines.YOU_DIED_TEXT;

        gameUIView.RoundCompleteText.SetText(string.Format(UIDefines.ROUND_COMPLETE_TEXT, roundIndex));
        gameUIView.GameOverText.SetText(gameOverBodyText);

        NavigateToPanel(2, false, () => { gameUIView.NextRoundButton.interactable = true; });
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

        SetPauseMenuButtonsInteractivity(false);

        MovePanel(gameUIView.PausePanel, targetPausePanelPosition, 0.5f, ()=> SetPauseMenuButtonsInteractivity(true));
    }

    private void HidePausePanel(bool canSetImmediate = false)
    {
        var panelHeight = gameUIView.PanelRoot.rect.height;
        targetPausePanelPosition = new Vector2(0, -panelHeight);

        if (!canSetImmediate)
        {
            gameUIAudioSource.PlayOneShot(gameUIView.PauseMenuOutSFX, 1);
            SetPauseMenuButtonsInteractivity(false);
            MovePanel(gameUIView.PausePanel, targetPausePanelPosition, 0.5f, () => SetPauseMenuButtonsInteractivity(true));            
        }
        else
        {
            gameUIView.PausePanel.anchoredPosition = targetPausePanelPosition;
        }
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed && !isPaused)
        {
            isPaused = true;
            gameUIView.PauseMenuResumeButton.interactable = isPaused;
            gameUIView.PauseMenuExitButton.interactable = isPaused;
            gameUIView.PauseMenuExitAppButton.interactable = isPaused;
            FadeBackground(0, 1, 0.5f);
            ShowPausePanel();
        }
        else if (value.isPressed && isPaused)
        {
            isPaused = false;
            gameUIView.PauseMenuResumeButton.interactable = isPaused;
            gameUIView.PauseMenuExitButton.interactable = isPaused;
            gameUIView.PauseMenuExitAppButton.interactable = isPaused;
            FadeBackground(1, 0, 0.5f);
            HidePausePanel();
        }
    }

    private void SetPauseMenuButtonsInteractivity(bool isInteractive)
    {
        gameUIView.PauseMenuExitAppButton.interactable = isInteractive;
        gameUIView.PauseMenuExitButton.interactable = isInteractive;
        gameUIView.PauseMenuResumeButton.interactable = isInteractive;
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

    private void SetStartButtonText()
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

    private async void FadeBackground(float startAlpha, float targetAlpha, float timeToReachTarget = 2.0f)
    {
        float animTime = 0;

        while (Mathf.Abs(animTime - timeToReachTarget) > 0.01f)
        {
            animTime += Time.deltaTime;

            var newAlpha = Mathf.Lerp(startAlpha, targetAlpha, animTime / timeToReachTarget);
            gameUIView.PanelBackgroundCanvasGroup.alpha = newAlpha;

            await Task.Yield();
        }

    }
}
