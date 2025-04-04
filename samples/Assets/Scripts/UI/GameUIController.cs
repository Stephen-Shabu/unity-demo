using System;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class GameUIController : MonoBehaviour
{
    public Action<Action>OnStartButtonPressed;
    public Action OnNextRoundButtonPressed;
    public Action OnSettingsButtonPressed;
    public Action OnDebugFinishRoundButtonPressed;
    public Action OnDebugFinishGameButtonPressed;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameUIView gameUIView;
    [SerializeField] private XpMeterController xpController;

    public float transitionSpeed = 10f;
    private Vector2 targetPosition;
    private Vector2 targetHUDPosition;
    private Vector2 targetPausePanelPosition;
    private bool isPaused;

    private const string START_TEXT = "START";
    private const string RESUME_TEXT = "RESUME";
    private const string ROUND_COMPLETE_TEXT = "Well Done! \nYou completed round {0}";

    public void Init()
    {
        gameUIView.PanelBackgroundCanvasGroup.alpha = 1f;
        targetPosition = gameUIView.PanelRoot.anchoredPosition;
        xpController.Initialise();
        SetStartButtonText();

        gameUIView.NextRoundButton.onClick.AddListener(() =>
        { 
            OnNextRoundButtonPressed?.Invoke();
            NavigateToPanel(1, true);
        });

        gameUIView.StartButton.onClick.AddListener(() =>
        {
            gameUIView.StartButton.interactable = false;
            gameUIView.SettingsButton.interactable = false;
            OnStartButtonPressed?.Invoke(() => { NavigateToPanel(1);  FadeBackground(1, 0); }); 
        });
        
        gameUIView.SettingsButton.onClick.AddListener(() => 
        {
            gameUIView.SettingsBackButton.interactable = true;
            gameUIView.SettingsButton.interactable = false;
            OnSettingsButtonPressed?.Invoke();
            NavigateToPanel(-1);
        });

        gameUIView.SettingsBackButton.onClick.AddListener(() =>
        {
            gameUIView.SettingsBackButton.interactable = false;
            gameUIView.SettingsButton.interactable = true;
            NavigateToPanel(0);
        });

        gameUIView.GameCompleteExitButton.onClick.AddListener(() =>
        {
            gameUIView.GameCompleteExitButton.interactable = false;
            gameUIView.StartButton.interactable = true;
            gameUIView.SettingsButton.interactable = true;
            NavigateToPanel(0);
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
        HidePausePanel();
    }

    public void SetResultPanel(bool isGameComplete, int roundNumber)
    {
        gameUIView.RoundCompletePanel.SetActive(!isGameComplete);
        gameUIView.GameCompletePanel.SetActive(isGameComplete);
        gameUIView.RoundCompleteText.SetText(string.Format(ROUND_COMPLETE_TEXT, roundNumber));
    }

    public void UpdateGameUI()
    {
        gameUIView.PanelRoot.anchoredPosition = Vector2.Lerp(gameUIView.PanelRoot.anchoredPosition, targetPosition, Time.deltaTime * transitionSpeed);
        gameUIView.PausePanel.anchoredPosition = Vector2.Lerp(gameUIView.PausePanel.anchoredPosition, targetPausePanelPosition, Time.deltaTime * transitionSpeed);
        xpController.XpMeterView.Root.anchoredPosition = Vector2.Lerp(xpController.XpMeterView.Root.anchoredPosition, targetHUDPosition, Time.deltaTime * transitionSpeed);
    }

    public void AnimateXpMeter(float amount, Action onComplete)
    {
        ShowHUD();
        xpController.AddExperience(amount, onComplete);
    }

    public void ShowHUD()
    {
        var hudHeight = xpController.XpMeterView.Root.rect.height;
        targetHUDPosition = new Vector2(0, hudHeight);
    }

    public void HideHUD()
    {
        var hudHeight = xpController.XpMeterView.Root.rect.height;
        targetHUDPosition = new Vector2(0, -hudHeight);
    }

    public void ShowPausePanel()
    {
        var panelHeight = xpController.XpMeterView.Root.rect.height;
        targetPausePanelPosition = new Vector2(0, 0);
    }

    public void HidePausePanel()
    {
        var panelHeight = gameUIView.PanelRoot.rect.height;
        targetPausePanelPosition = new Vector2(0, -panelHeight);
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed && !isPaused)
        {
            isPaused = true;
            gameUIView.PauseMenuResumeButton.interactable = isPaused;
            gameUIView.PauseMenuExitButton.interactable = isPaused;
            gameUIView.PauseMenuExitAppButton.interactable = isPaused;
            FadeBackground(0, 1);
            ShowPausePanel();
        }
        else if (value.isPressed && isPaused)
        {
            isPaused = false;
            gameUIView.PauseMenuResumeButton.interactable = isPaused;
            gameUIView.PauseMenuExitButton.interactable = isPaused;
            gameUIView.PauseMenuExitAppButton.interactable = isPaused;
            FadeBackground(1, 0);
            HidePausePanel();
        }
    }

    public void NavigateToPanel(int panelIndex, bool canSetImmediate = false)
    {
        if (!canSetImmediate)
        {
            float canvasWidth = gameUIView.PanelRoot.rect.width;
            targetPosition = new Vector2(-canvasWidth * panelIndex, 0);
        }
        else
        {
            float canvasWidth = gameUIView.PanelRoot.rect.width;
            targetPosition = new Vector2(-canvasWidth * panelIndex, 0);
            gameUIView.PanelRoot.anchoredPosition = targetPosition;
        }
    }

    private void SetStartButtonText()
    {
        var profile = Main.Instance.ProfileCache;
        var title = profile.LastCompletedLevel > 0 ? RESUME_TEXT : START_TEXT;
        gameUIView.StartButtonText.text = string.Format(gameUIView.StartButtonText.text, title);
    }

    private async void FadeBackground(float startAlpha, float targetAlpha)
    {
        float animTime = 0;
        float timeToReachTarget = 2.0f;

        while (Mathf.Abs(animTime - timeToReachTarget) > 0.01f)
        {
            animTime += Time.deltaTime;

            var newAlpha = Mathf.Lerp(startAlpha, targetAlpha, animTime / timeToReachTarget);
            gameUIView.PanelBackgroundCanvasGroup.alpha = newAlpha;

            await Task.Yield();
        }
    }
}
