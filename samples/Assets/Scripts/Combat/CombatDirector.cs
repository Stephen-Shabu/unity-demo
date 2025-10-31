using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface AttackCapable
{
    void Wait(Transform target);
    void Attack(Transform target);
    void Retreat();
}

public interface Attackable
{
    int GetMaxAttackers();
}

public class CombatDirector : MonoBehaviour
{
    private GameObject target;
    private List<GameObject> attackers = new List<GameObject>();
    private List<GameObject> waitingAttackers = new List<GameObject>();
    private int maxAttackers;

    public void Initialise(GameObject attackTarget)
    {
        target = attackTarget;

        if (target.TryGetComponent(out Attackable targetController))
        {
            maxAttackers = targetController.GetMaxAttackers();
        }

        GameEventsEmitter.OnEvent(EventType.RequestAttack, RequestAttack);
        GameEventsEmitter.OnEvent(EventType.PlayerDefeated, HandlePlayerDefeated);
        GameEventsEmitter.OnEvent(EventType.EnemyDefeated, HandleEnemyDefeated);
        GameEventsEmitter.OnEvent(EventType.ChangeState, HandleStateChange);
    }

    public void RequestAttack(EventData e)
    {
        AttackRequestEventData data;

        if (e is AttackRequestEventData value)
        {
            data = value;

            if (target.GetInstanceID() != data.Attacker.GetInstanceID())
            {
                if (attackers.Count < maxAttackers)
                {
                    attackers.Add(data.Attacker);

                    if (data.Attacker.TryGetComponent(out AttackCapable attackerController))
                    {
                        attackerController.Attack(target.transform);
                    }
                }
                else
                {
                    waitingAttackers.Add(data.Attacker);

                    if (data.Attacker.TryGetComponent(out AttackCapable attackerController))
                    {
                        attackerController.Wait(target.transform);
                    }
                }
            }
        }
    }

    public void AddWaitingAttacker()
    {
        if (waitingAttackers.Count > 0)
        {
            var attacker = waitingAttackers.First();
            attackers.Add(attacker);

            waitingAttackers.Remove(attacker);

            if (attacker.TryGetComponent(out AttackCapable attackerController))
            {
                attackerController.Attack(target.transform);
            }
        }
    }

    private void HandlePlayerDefeated(EventData e)
    {
        waitingAttackers.Clear();
        attackers.Clear();
    }

    private void HandleEnemyDefeated(EventData e)
    {
        GenericEventData data;

        if (e is GenericEventData value)
        {
            var attackersArr = attackers.ToList();

            data = value;
            var mobGO = data.Caller;

            if (attackers.Contains(mobGO))
            {
                attackersArr.Remove(mobGO);

                AddWaitingAttacker();
            }
            else
            {
                waitingAttackers.Remove(mobGO);
            }    
        }
    }

    private void HandleStateChange(EventData e)
    {
        StateEventData data;

        if (e is StateEventData value)
        {
            data = value;

            switch (data.State)
            {
                case GameState.Results:
                    waitingAttackers.Clear();
                    attackers.Clear();
                    break;
            }
        }
    }
}
