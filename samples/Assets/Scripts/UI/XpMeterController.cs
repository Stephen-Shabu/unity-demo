using UnityEngine;
using System.Threading.Tasks;

public class XpMeterController : MonoBehaviour
{
    public int[] XpPerLevel = new int[] { 15, 22, 25, 32, 37, 44, 50, 67, 76, 81, 101, 130, 150, 202, 287, 320, 380, 410, 500, 600 };
    public int[] XpAdditions = new int[] { 50, 10, 15, 5, 100, 3, 18};

    [SerializeField] private XpMeterView xpMeterView;
    [SerializeField] private float fillSpeed = 1.0f;
    [SerializeField] private float xpGainSpeed = 1.0f;
    [SerializeField] private float currentExperience = 0.0f;
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private AnimationCurve animCurve;
    [SerializeField] private AnimationCurve xpGainPositionCurve;

    private int expFloor;
    private int expCeil;
    private int callCount;
    private bool isUpdating;

    private void Start()
    {
        xpMeterView.AddXpButton.onClick.AddListener(() => 
        {
            if (isUpdating) return;

            isUpdating = true;
            var expAmount = XpAdditions[callCount % XpAdditions.Length];
            callCount++;
            AddExperience(expAmount);
        });
    }

    private void InitializeExperienceBar(float currentExp)
    {
        xpMeterView.XpLevelText.text = $"{currentLevel}";
        xpMeterView.XpGainedTextCanvasGroup.alpha = 0;
        if (currentLevel < XpPerLevel.Length)
        {
            currentExperience = currentExp;
            expFloor = GetExpFloor(currentLevel);
            expCeil = GetExpFloor(currentLevel + 1);
            var amount = Mathf.InverseLerp(expFloor, expCeil, currentExperience);
            xpMeterView.XpBarImage.fillAmount = amount;
        }
        else
        {
            Debug.LogError("Experience levels array is too small for the current level!");
        }
    }

    public void AddExperience(float experienceAmount)
    {
        InitializeExperienceBar(currentExperience);
        UpdateExperienceBarAsync(experienceAmount);
    }

    private async void UpdateExperienceBarAsync(float experienceAmount)
    {
        float targetExperience = currentExperience + experienceAmount;
        float exp = experienceAmount;

        AnimateXpGained(targetExperience, currentExperience, experienceAmount);

        while (!Mathf.Approximately(currentExperience, targetExperience))
        {
            if (currentExperience >= expCeil)
            {
                LevelUp();
            }

            float newExp = Mathf.MoveTowards(currentExperience, targetExperience, fillSpeed * Time.deltaTime);
            experienceAmount -= newExp - currentExperience;
            //var expGained = targetExperience - (targetExperience - exp) - experienceAmount;
            var amount = Mathf.InverseLerp(expFloor, expCeil, currentExperience);
            xpMeterView.XpBarImage.fillAmount = amount;
            currentExperience = newExp;

            await Task.Yield();
        }
        isUpdating = false;
    }

    private async void AnimateXpGained(float targetXp, float currentXp, float amount)
    {
        float animTime = 0;
        float cachedGain = amount;
        float distance = Mathf.Abs(targetXp - currentXp);
        float timeToReachTarget = xpGainSpeed;
        float xp = currentXp;

        while (Mathf.Abs(animTime - timeToReachTarget) > 0.01f)
        {
            animTime += Time.deltaTime;
            var newXp = Mathf.Lerp(xp, targetXp, animTime / (timeToReachTarget * .25f));
            Debug.Log(newXp);
            amount -= newXp - xp;
            var expGained = targetXp - (targetXp - cachedGain) - amount;
            xpMeterView.XpGained.text = string.Format("+{0}", (int)expGained);
            xpMeterView.XpGainedTextCanvasGroup.alpha = animCurve.Evaluate(animTime / timeToReachTarget);

            RectTransform xpGainedRect = xpMeterView.XpGained.GetComponent<RectTransform>();
            xpGainedRect.anchoredPosition = new Vector2(xpGainPositionCurve.Evaluate(animTime / timeToReachTarget), 0);

            xp = newXp;

            await Task.Yield();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        xpMeterView.XpLevelText.text = $"{currentLevel}";

        if (currentLevel < XpPerLevel.Length)
        {
            expFloor = GetExpFloor(currentLevel);
            expCeil = GetExpFloor(currentLevel + 1);
            var amount = Mathf.InverseLerp(expFloor, expCeil, currentExperience);
            xpMeterView.XpBarImage.fillAmount = amount;
        }
        else
        {
            Debug.Log("Max level reached!");
            currentLevel--; // Ensure current level stays within bounds
        }
    }

    private int GetExpFloor(int level)
    {
        int lastXpFloor = 0;

        for (int i = 0; i < level; i++)
        {
            lastXpFloor += XpPerLevel[i];
        }

        return lastXpFloor;
    }
}
