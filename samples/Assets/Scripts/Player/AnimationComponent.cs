using System;
using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Action<Animator> updateAnimator;
    private Action<Animator> updateAnimatorOnce;

    public void SetAnimUpdateCallback(Action<Animator> callback)
    {
        updateAnimator = callback;
    }

    public void SetAnimUpatedOneShot(Action<Animator> callback)
    {
        updateAnimatorOnce = callback;
    }

    public void ApplyAnimation()
    {
        updateAnimator?.Invoke(animator);
    }

    public void ApplyOneShotAnimation()
    {
        updateAnimatorOnce?.Invoke(animator);
    }   
}
