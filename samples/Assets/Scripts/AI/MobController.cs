using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;

public class MobController : MonoBehaviour
{
    [SerializeField] private Vector2 inputVector = Vector2.zero;
    [SerializeField] private Vector2 lookVector = Vector2.zero;
    [SerializeField] private Vector3 heading = Vector3.zero;
    [Range(0, 1)][SerializeField] private float intensity = 1;
    [SerializeField] private Transform target;
    [SerializeField] private MovementComponent moveComponent;

    private void FixedUpdate()
    {
        var moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        if (Vector3.Distance(transform.position, target.position) > 5)
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
