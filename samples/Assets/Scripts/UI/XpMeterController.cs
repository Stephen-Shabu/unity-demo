using UnityEngine;
using System.Threading.Tasks;
using System;

public class XpMeterController : MonoBehaviour
{
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

    public void Initialise()
    {
        var profile = Main.Instance.ProfileCache;

        if (profile != null)
        {
            currentExperience = profile.XpTotal;
            currentLevel = profile.XpLevel;
            Debug.Log($"Init xp bar {currentExperience} {currentLevel}");
        }

        InitializeExperienceBar(currentExperience);
    }

    public void AddExperience(float experienceAmount, Action onComplete = null)
    {
        InitializeExperienceBar(currentExperience);
        UpdateExperienceBarAsync(experienceAmount, onComplete);
    }

    private void InitializeExperienceBar(float currentExp)
    {
        xpMeterView.XpLevelText.text = $"{currentLevel}";
        xpMeterView.XpGainedTextCanvasGroup.alpha = 0;
        if (currentLevel < RewardDefines.XP_PER_LEVEL.Length)
        {
            currentExperience = currentExp;
            expFloor = GetExpFloor(currentLevel);
            expCeil = GetExpFloor(currentLevel + 1);
            var amount = Mathf.InverseLerp(expFloor, expCeil, currentExperience);
            xpMeterView.XpBarImage.fillAmount = amount;
        }
    }

    private async void UpdateExperienceBarAsync(float experienceAmount, Action onComplete = null)
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
            var amount = Mathf.InverseLerp(expFloor, expCeil, currentExperience);
            xpMeterView.XpBarImage.fillAmount = amount;
            currentExperience = newExp;

            await Task.Yield();
        }
        isUpdating = false;

        await Task.Delay(1 * MathDefines.MILLISECOND_MULTIPLIER);

        onComplete?.Invoke();
    }

    private async void AnimateXpGained(float targetXp, float currentXp, float amount)
    {
        float animTime = 0;
        float cachedGain = amount;
        float timeToReachTarget = xpGainSpeed;
        float xp = currentXp;

        while (Mathf.Abs(animTime - timeToReachTarget) > 0.01f)
        {
            animTime += Time.deltaTime;
            var newXp = Mathf.Lerp(xp, targetXp, animTime / (timeToReachTarget * .25f));

            amount -= newXp - xp;
            var expGained = targetXp - (targetXp - cachedGain) - amount;
            xpMeterView.XpGained.text = string.Format(RewardDefines.XP_GAINED_FORMAT, (int)expGained);
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

        if (currentLevel < RewardDefines.XP_PER_LEVEL.Length)
        {
            expFloor = GetExpFloor(currentLevel);
            expCeil = GetExpFloor(currentLevel + 1);
            var amount = Mathf.InverseLerp(expFloor, expCeil, currentExperience);
            xpMeterView.XpBarImage.fillAmount = amount;
        }
        else
        {
            Debug.Log("Max level reached!");
            currentLevel--;
        }
    }

    private int GetExpFloor(int level)
    {
        int lastXpFloor = 0;

        for (int i = 0; i < level; i++)
        {
            lastXpFloor += RewardDefines.XP_PER_LEVEL[i];
        }

        return lastXpFloor;
    }
}
