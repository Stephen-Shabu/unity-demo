using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CharactorController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Vector2 inputVector = Vector2.zero;
    [SerializeField] private Vector2 lookVector = Vector2.zero;
    [SerializeField] private bool hasJumped = false;
    [SerializeField] private MovementComponent moveComponent;
    [SerializeField] private BaseCameraComponent cameraComponent;

    public void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookVector = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        hasJumped = value.isPressed;
        moveComponent.ApplyJumpVelocity(hasJumped);
    }

    private void FixedUpdate()
    {
        Vector3 cameraForward = cameraComponent.transform.forward;
        Vector3 cameraRight = cameraComponent.transform.right;
        cameraForward.y = 0.0f;

        Vector3 movementDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x).normalized;

        moveComponent.UpdateMovement(movementDirection);
        cameraComponent.TrackPlayer(transform, lookVector);
    }

    private void LateUpdate()
    {
        //cameraComponent.TrackPlayer(transform, lookVector);
    }
}
