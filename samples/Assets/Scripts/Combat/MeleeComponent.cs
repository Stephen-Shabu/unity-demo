using UnityEngine;
using System.Threading.Tasks;
using System;

public class MeleeComponent : MonoBehaviour
{
    [SerializeField] private Rigidbody attactedRigidBody;

    public void LaunchMeleeAttack(Vector3 direction, Action attackComplete = null)
    {
        //Launch(direction, attackComplete);
    }

    public void CancelMeleeAttack()
    {
        
    }

    private async Task Launch(Vector3 direction, Action attackComplete = null)
    {
        await Task.Delay(1000);

        attactedRigidBody.linearVelocity += direction * 15f;
        attackComplete?.Invoke();
    }
}
