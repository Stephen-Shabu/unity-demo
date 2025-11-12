using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mono.Cecil.Cil;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime;

public class MeleeComponent : MonoBehaviour
{
    public bool CanAttack => !canLaunchAttack && currentCooldown == attackCooldownTime;

    [SerializeField] private Rigidbody attactedRigidBody;
    [SerializeField] private float attackSpeed = 10f;
    [SerializeField] private float attackDistance = 4f;
    [SerializeField] private float attackCooldownTime;
    [SerializeField] LayerMask collionLayer;

    private Action onAttackComplete;
    private bool canLaunchAttack = false;
    private float currentCooldown = 5;
    private IEnumerator IERunCoolDownTimer;
    private IEnumerator IEActivateHitBox;
    private Coroutine activateHitBoxRoutine;
    private Coroutine launchRoutine;
    private Coroutine coolDownRoutine;
    private Collider[] hits = new Collider[1];
    private HashSet<Collider> alreadyHit = new();

    public void LaunchMeleeAttack(Vector3 direction, Action onAttackComplete)
    {
        if (!canLaunchAttack && currentCooldown >= attackCooldownTime)
        {
            canLaunchAttack = true;
            currentCooldown = 0;
            if(launchRoutine != null) StopCoroutine(launchRoutine);
            launchRoutine = StartCoroutine(Launch(direction, onAttackComplete));
        }
    }

    public void CancelMeleeAttack()
    {
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

    private IEnumerator Launch(Vector3 direction, Action onAttackComplete)
    {
        this.onAttackComplete = onAttackComplete;

        direction.y = 0;

        var start = transform.position;
        var end = start + direction * attackDistance;

        var startTime = Time.time;
        var distance = Vector3.Distance(transform.position, end);
        var time = Mathf.Max(0.01f, distance / attackSpeed);

        while (Vector3.Distance(transform.position, end) > 0.05f)
        {
            float elapsed = Time.time - startTime;
            var fracComplete = Mathf.Clamp01(elapsed / time);
            var easedT = Mathf.SmoothStep(0f, 1f, fracComplete);
            Vector3 newPosition = Vector3.Lerp(start, end, easedT);
            attactedRigidBody.MovePosition(newPosition);

            if (activateHitBoxRoutine != null) StopCoroutine(activateHitBoxRoutine);

            activateHitBoxRoutine = StartCoroutine(ActivateHitBox(direction));

            yield return new WaitForFixedUpdate();
        }

        this.onAttackComplete?.Invoke();

        canLaunchAttack = false;

        if (activateHitBoxRoutine != null) StopCoroutine(activateHitBoxRoutine);
        alreadyHit.Clear();

        if (coolDownRoutine != null) StopCoroutine(coolDownRoutine);
        coolDownRoutine = StartCoroutine(CoolDownAttack());
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

                    this.onAttackComplete?.Invoke();
                    canLaunchAttack = false;

                    if (launchRoutine != null) StopCoroutine(launchRoutine);
                    if (coolDownRoutine != null) StopCoroutine(coolDownRoutine);

                    coolDownRoutine = StartCoroutine(CoolDownAttack());

                    attactedRigidBody.linearVelocity += direction * MovementDefines.Character.KNOCKBACK_DISTANCE;

                    GameEventsEmitter.EmitEvent(EventType.MeleeHitRegistered, new GenericEventData { Type = EventType.MeleeHitRegistered });

                    alreadyHit.Clear();
                    if (activateHitBoxRoutine != null) StopCoroutine(activateHitBoxRoutine);
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
