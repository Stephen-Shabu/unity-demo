using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class MeleeComponent : MonoBehaviour
{
    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private float attackCooldownTime;
    [SerializeField] LayerMask collionLayer;

    private bool canLaunchAttack = false;
    private float currentCooldown = 5;
    private IEnumerator IERunCoolDownTimer;
    private IEnumerator IEActivateHitBox;
    private Coroutine activateHitBoxRoutine;
    private Coroutine coolDownRoutine;
    private Collider[] hits = new Collider[1];
    private HashSet<Collider> alreadyHit = new();
    private CancellationTokenSource launchAttackCTS;

    public bool CanAttack(bool isInDistance)
    {
        return !canLaunchAttack && isInDistance;
    }

    public void LaunchMeleeAttack(Transform target, Action onAttackStartUp, Action onAttackStarted, Action onAttackComplete)
    {
        if (!canLaunchAttack && currentCooldown >= attackCooldownTime)
        {
            canLaunchAttack = true;
            currentCooldown = 0;

            Launch(target, onAttackStartUp, onAttackStarted, onAttackComplete);
        }
    }

    public void CancelMeleeAttack()
    {
        launchAttackCTS?.Cancel();

        if (activateHitBoxRoutine != null)
        {
            StopCoroutine(activateHitBoxRoutine);
            activateHitBoxRoutine = null;
        }
        if (coolDownRoutine != null)
        {
            StopCoroutine(coolDownRoutine);
            coolDownRoutine = null;
        }

        canLaunchAttack = false;
        alreadyHit.Clear();
    }

    private IEnumerator CoolDownAttack()
    {
        while (currentCooldown < attackCooldownTime)
        {
            currentCooldown += Time.deltaTime;

            yield return null;
        }
        currentCooldown = attackCooldownTime;
    }

    private async Task Launch(Transform target, Action onAttackStartUp, Action onAttackStarted, Action onAttackComplete)
    {
        launchAttackCTS?.Cancel();
        launchAttackCTS = new CancellationTokenSource();
        var token = launchAttackCTS.Token;

        try
        {
            token.ThrowIfCancellationRequested();

            onAttackStartUp?.Invoke();

            await Task.Delay(1 * MathDefines.MILLISECOND_MULTIPLIER, token);

            onAttackStarted?.Invoke();
            var direction = (target.position - transform.position).normalized;
            direction.y = 0;

            attactedRigidBody.linearVelocity += direction * 30f;

            if (activateHitBoxRoutine != null) StopCoroutine(activateHitBoxRoutine);

            activateHitBoxRoutine = StartCoroutine(ActivateHitBox(direction));

            await Task.Delay(1 * MathDefines.MILLISECOND_MULTIPLIER, token);

            onAttackComplete?.Invoke();

            canLaunchAttack = false;

            if (activateHitBoxRoutine != null) StopCoroutine(activateHitBoxRoutine);
            alreadyHit.Clear();

            if (coolDownRoutine != null) StopCoroutine(coolDownRoutine);
            coolDownRoutine = StartCoroutine(CoolDownAttack());
        }
        catch (OperationCanceledException)
        {

        }
    }

    private IEnumerator ActivateHitBox(Vector3 direction)
    {
        var rotation = Quaternion.LookRotation(direction);

        while (true)
        {
            Vector3 box = new Vector3(.5f, 0.3f, 0.3f);
            int hit = Physics.OverlapBoxNonAlloc(transform.position + (transform.forward * .2f), box, hits, rotation, collionLayer);

            DebugExtension.DrawDebugBox(transform.position + (transform.forward * .2f), box, rotation, Color.cyan, Time.fixedDeltaTime);

            if (hit > 0 && hits[0] != null)
            {
                if (hits[0].TryGetComponent(out HealthComponent healthComp) && !alreadyHit.Contains(hits[0]))
                {                    
                    healthComp.ReactToHit(direction);
                    alreadyHit.Add(hits[0]);
                    attactedRigidBody.linearVelocity += direction * -20f;

                    GameEventsEmitter.EmitEvent(EventType.MeleeHitRegistered, new GenericEventData { Type = EventType.MeleeHitRegistered });
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDisable()
    {
        CancelMeleeAttack();
    }
}
