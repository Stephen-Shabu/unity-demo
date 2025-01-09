using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Samples
{
    public class CharactorController : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Vector2 inputVector = Vector2.zero;
        [SerializeField] private Vector2 lookVector = Vector2.zero;
        [SerializeField] private CharacterMovement moveComponent;
        [SerializeField] private BaseCameraComponent cameraComponent;
        [SerializeField] private ProjectileComponent projectileComponent;

        [SerializeField] private bool hasJumped = false;
        [SerializeField] private bool hasAttacked;
        [SerializeField] private bool hasDodged;

        public void Initialise(BaseCameraComponent camera)
        {
            cameraComponent = camera;
            moveComponent.Intialise();
            projectileComponent.Initialise();
        }

        public void OnAttack(InputValue value)
        {
            hasAttacked = value.isPressed;
            projectileComponent.Fire(hasAttacked);
        }

        public void OnDodge(InputValue value)
        {
            Debug.Log("Dodge");
            hasDodged = value.isPressed;
            moveComponent.ApplyDogde(hasDodged);
        }

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

        public void UpdateController()
        {
            Vector3 cameraForward = cameraComponent.transform.forward;
            Vector3 cameraRight = cameraComponent.transform.right;
            cameraForward.y = 0.0f;

            Vector3 movementDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x).normalized;

            moveComponent.UpdateMovement(movementDirection, hasAttacked);
            cameraComponent.TrackPlayer(transform, lookVector);
            projectileComponent.UpdateComponent();
        }
    }
}
