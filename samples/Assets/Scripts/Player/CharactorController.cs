using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Samples
{
    public class CharactorController : MonoBehaviour, Attackable
    {
        public Action OnHealthReachedZero;

        [SerializeField] private Vector2 inputVector = Vector2.zero;
        [SerializeField] private Vector2 lookVector = Vector2.zero;
        [SerializeField] private AnimationComponent animComponent;
        [SerializeField] private CharacterMovement moveComponent;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private BaseCameraComponent cameraComponent;
        [SerializeField] private ProjectileComponent projectileComponent;

        [SerializeField] private bool hasJumped = false;
        [SerializeField] private bool hasAttacked;
        [SerializeField] private bool hasDodged;

        private PlayerInput playerInput;

        public void Initialise(BaseCameraComponent camera, PlayerInput input)
        {
            playerInput = input;
            cameraComponent = camera;
            moveComponent.Intialise();
            projectileComponent.Initialise();
            healthComponent.Initialise();

            healthComponent.OnHealthReachedZero = HandleOnHealthReachedZero;
            healthComponent.OnDeathComplete = HandleDeathComplete;
            healthComponent.OnDamageRecieved = HandleOnDamageTaken;

            playerInput.actions["Attack"].performed -= OnAttack;
            playerInput.actions["Attack"].performed += OnAttack;
            playerInput.actions["Dodge"].performed -= OnDodge;
            playerInput.actions["Dodge"].performed += OnDodge;
            playerInput.actions["LeftDodge"].performed -= OnLeftDodge;
            playerInput.actions["LeftDodge"].performed += OnLeftDodge;
            
            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled -= OnMove;
            playerInput.actions["Move"].canceled += OnMove;

            playerInput.actions["Look"].performed -= OnLook;
            playerInput.actions["Look"].performed += OnLook;
            playerInput.actions["Look"].canceled -= OnLook;
            playerInput.actions["Look"].canceled += OnLook;
        }

        private void HandleOnDamageTaken()
        {
            animComponent.SetHitParameter(true);
        }

        private void HandleOnHealthReachedZero()
        {
            GameEventsEmitter.EmitEvent(EventType.PlayerDefeated, new GenericEventData { Type = EventType.PlayerDefeated });
        }

        private void HandleDeathComplete()
        {
            OnHealthReachedZero?.Invoke();
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            hasAttacked = context.action.IsPressed();
            projectileComponent.Fire(hasAttacked);
            animComponent.SetFiring(hasAttacked);
        }

        private void OnDodge(InputAction.CallbackContext context)
        {
            Debug.Log("Dodge");
            hasDodged = context.action.IsPressed();
            moveComponent.ApplyDogde(hasDodged);
        }

        private void OnLeftDodge(InputAction.CallbackContext context)
        {
            Debug.Log("Dodge");
            hasDodged = context.action.IsPressed();
            moveComponent.ExecuteLeftDogde(hasDodged);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            inputVector = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            lookVector = context.ReadValue<Vector2>();
        }

        public void UpdateController()
        {
            Vector3 cameraForward = cameraComponent.transform.forward;
            Vector3 cameraRight = cameraComponent.transform.right;
            cameraForward.y = 0.0f;

            Vector3 movementDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x).normalized;
            moveComponent.UpdateMovement(movementDirection, hasAttacked);
            animComponent.SetMovementParameter(moveComponent.IsMoving, moveComponent.SpeedPercentage);
            cameraComponent.TrackPlayer(transform, lookVector);
            projectileComponent.UpdateComponent();
        }

        private void OnDestroy()
        {
            if (playerInput != null)
            {
                playerInput.actions["Attack"].performed -= OnAttack;
                playerInput.actions["Dodge"].performed -= OnDodge;
                playerInput.actions["LeftDodge"].performed -= OnLeftDodge;
                playerInput.actions["Move"].performed -= OnMove;
                playerInput.actions["Move"].canceled -= OnMove;
                playerInput.actions["Look"].performed -= OnLook;
                playerInput.actions["Look"].canceled -= OnLook;            
            }
        }

        public int GetMaxAttackers()
        {
            return 3;
        }
    }
}
