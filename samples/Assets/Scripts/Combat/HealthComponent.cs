using UnityEngine;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting;

public class HealthComponent : MonoBehaviour
{
    public event Action<Vector3, int> OnDamageTaken;
    public event Action<Vector3> OnDeathStarted;
    public event Action OnDeathFinished;
    public bool IsHealthZero { get; private set; }

    [SerializeField] private int maxHealth = 4;

    private int currentHealth = 0;

    public void Initialise()
    {
        currentHealth = maxHealth;
    }

    public void ReactToHit(Vector3 hitDirection)
    {   
        if (currentHealth > 0)
            currentHealth -= 1;

        OnDamageTaken?.Invoke(hitDirection, currentHealth);

        if (currentHealth == 0)
        {
            OnDeathStarted?.Invoke(hitDirection);
        }

        IsHealthZero = currentHealth == 0;
    }

    public void NotifyDeathComplete() => OnDeathFinished?.Invoke();
}
