using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIView : MonoBehaviour
{
    public CanvasGroup PanelBackgroundCanvasGroup => panelBackgroundCanvasGroup;
    public RectTransform PanelRoot => panelRoot;
    public RectTransform HomePanel => homePanel;
    public RectTransform GamePanel => gamePanel;
    public RectTransform ResultPanel => resultPanel;
    public GameObject RoundCompletePanel => roundCompletePanel;
    public GameObject GameCompletePanel => gameCompletePanel;
    public Button StartButton => startButton;
    public Button SettingsButton => settingsButton;
    public Button SettingsBackButton => settingsBackButton;
    public Button NextRoundButton => nextRoundButton;
    public Button GameCompleteExitButton => gameCompleteExitButton;
    public Button PauseMenuExitButton => pauseMenuExitButton;
    public Button DebugFinishRoundButton => debugFinishRoundButton;
    public Button DebugFinishGameButton => debugGameRoundButton;
    public TextMeshProUGUI StartButtonText => startButtonText;
    public TextMeshProUGUI RoundCompleteText => roundCompleteText;

    [SerializeField] private CanvasGroup panelBackgroundCanvasGroup;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private RectTransform homePanel;
    [SerializeField] private RectTransform gamePanel;
    [SerializeField] private RectTransform resultPanel;
    [SerializeField] private GameObject roundCompletePanel;
    [SerializeField] private GameObject gameCompletePanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private Button gameCompleteExitButton;
    [SerializeField] private Button pauseMenuExitButton;
    [SerializeField] private TextMeshProUGUI startButtonText;
    [SerializeField] private TextMeshProUGUI roundCompleteText;
    [SerializeField] private Button debugFinishRoundButton;
    [SerializeField] private Button debugGameRoundButton;
}
