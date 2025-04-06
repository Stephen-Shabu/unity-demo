using UnityEngine;
using System.Collections;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color urgentTextColor;
    [SerializeField] private AudioSource timerAudioSource;
    [SerializeField] private AudioClip timerSFX;

    private IEnumerator IETimer;
    private WaitForSeconds _yieldTime;

    public void StartTimer(float time, System.Action<float> updateCallback,
        System.Action completeCallback = null)
    {
        if (IETimer != null)
        {
            StopCoroutine(IETimer);
        }
        _yieldTime = new WaitForSeconds(1);
        IETimer = Execute(time, updateCallback, completeCallback);
        StartCoroutine(IETimer);
    }

    private IEnumerator Execute(float time, System.Action<float> updateCallback,
        System.Action completeCallback = null)
    {
        float startTime = time;

        while (startTime > 0)
        {
            yield return _yieldTime;
            startTime -= 1;

            int minute = ((int)startTime / 60);
            _timerText.text = string.Format(GameDefines.TIMER_FORMAT, (int)startTime / 60, startTime % 60);

            if (startTime <= 5)
            {
                timerAudioSource.PlayOneShot(timerSFX);
                _timerText.color = urgentTextColor;
            }
            
            if (updateCallback != null)
                updateCallback(startTime);
        }

        if (completeCallback != null)
            completeCallback();
    }

    public void StopTimer()
    {
        if (IETimer != null)
        {
            StopCoroutine(IETimer);
        }
    }

    public void ResetTimeText(float time)
    {
        int minute = ((int) time / 60);
        _timerText.text = string.Format(GameDefines.TIMER_FORMAT, (int) time / 60, time % 60);
        _timerText.color = normalTextColor;
    }
}
