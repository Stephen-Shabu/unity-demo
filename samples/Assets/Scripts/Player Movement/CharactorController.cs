using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CharactorController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Vector2 inputVector = Vector2.zero;
    [SerializeField] private MovementComponent moveComponent;

    public void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        moveComponent.UpdateMovement(inputVector);
    }
}
