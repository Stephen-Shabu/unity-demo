using System.Data.Common;
using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetMovementParameter(bool isRunning, float moveBlend)
    {
        animator.SetBool("IsRunning", isRunning);
        animator.SetFloat("MovementBlend", moveBlend);
    }

    public void SetHitParameter(bool isHit)
    {
        animator.SetBool("IsHit", true);
        animator.SetBool("IsHit", false);
    }

    public void SetFiring(bool isFiring)
    {
        animator.SetBool("IsFiring", isFiring);
    }
}
