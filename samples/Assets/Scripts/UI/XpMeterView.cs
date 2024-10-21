using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XpMeterView : MonoBehaviour
{
    public Image XpBarImage => xpBarImage;
    public TextMeshProUGUI XpLevelText => xpLevelText;
    public TextMeshProUGUI XpGained => xpGained;
    public CanvasGroup XpGainedTextCanvasGroup => xpGainedTextCanvasGroup;
    public Button AddXpButton => addXpButton;

    [SerializeField] private Image xpBarImage;
    [SerializeField] private TextMeshProUGUI xpLevelText;
    [SerializeField] private TextMeshProUGUI xpGained;
    [SerializeField] private CanvasGroup xpGainedTextCanvasGroup;
    [SerializeField] private Button addXpButton;
}
