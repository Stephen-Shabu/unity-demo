using UnityEngine;

public class MobController : MonoBehaviour
{
    [SerializeField] private Vector2 inputVector = Vector2.zero;
    [SerializeField] private Vector2 lookVector = Vector2.zero;
    [SerializeField] private Vector3 heading = Vector3.zero;
    [Range(0, 1)][SerializeField] private float intensity = 1;
    [SerializeField] private float stoppingDistance = 5;
    [SerializeField] private Transform target;
    [SerializeField] private MovementComponent moveComponent;
    [SerializeField] private HealthComponent healthComponent;

    private bool hasHealthReachedZero = false;

    private void Start()
    {
        healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
    }

    private void HandleOnHealthReachedZero()
    {
        hasHealthReachedZero = !hasHealthReachedZero;
    }

    private void FixedUpdate()
    {
        var moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        if (!hasHealthReachedZero && Vector3.Distance(transform.position, target.position) > stoppingDistance)
        {
            heading = target.position - transform.position;
        }
        else
        {
            heading = Vector3.zero;
        }

        moveComponent.UpdateMovement(heading.normalized * intensity, false);
    }
}
