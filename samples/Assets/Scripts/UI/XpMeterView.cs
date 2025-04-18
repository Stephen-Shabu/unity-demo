using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XpMeterView : MonoBehaviour
{
    public RectTransform Root => root;
    public Image XpBarImage => xpBarImage;
    public TextMeshProUGUI XpLevelText => xpLevelText;
    public TextMeshProUGUI XpGained => xpGained;
    public CanvasGroup XpGainedTextCanvasGroup => xpGainedTextCanvasGroup;
    public AudioClip XpGainSFX => xpGainSFX;
    public AudioClip LevelUpFX => levelUpFX;
    public Button AddXpButton => addXpButton;

    [SerializeField] private RectTransform root;
    [SerializeField] private Image xpBarImage;
    [SerializeField] private TextMeshProUGUI xpLevelText;
    [SerializeField] private TextMeshProUGUI xpGained;
    [SerializeField] private CanvasGroup xpGainedTextCanvasGroup;
    [SerializeField] private AudioClip xpGainSFX;
    [SerializeField] private AudioClip levelUpFX;
    [SerializeField] private Button addXpButton;
}
