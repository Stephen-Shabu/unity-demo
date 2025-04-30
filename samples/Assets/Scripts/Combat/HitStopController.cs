using UnityEngine;
using System.Collections;

public class HitStopController : MonoBehaviour
{
    private Coroutine hitStopRoutine;
    private bool isHitStopping;

    public void Initialise()
    {
        GameEventsEmitter.OnEvent(EventType.MeleeHitRegistered, OnHitRegistered);
    }

    private void OnHitRegistered(EventData e)
    {
        TriggerHitStop(0.1f);
    }

    public void TriggerHitStop(float duration)
    {
        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
        }

        hitStopRoutine = StartCoroutine(StartHitStop(duration));
    }

    private IEnumerator StartHitStop(float duration)
    {
        isHitStopping = true;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        isHitStopping = false;
    }
}
