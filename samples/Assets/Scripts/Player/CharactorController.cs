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
        [SerializeField] private HealthFXComponent healthFxComponent;
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
            projectileComponent.ChangeWeapon(WeaponName.BLASTER);
            healthComponent.Initialise();
            healthFxComponent.Initialise();

            healthComponent.OnDamageTaken -= HandleOnDamageTaken;
            healthComponent.OnDamageTaken += HandleOnDamageTaken;
            healthComponent.OnDeathStarted -= HandleOnDeathStarted;
            healthComponent.OnDeathStarted += HandleOnDeathStarted;
            healthComponent.OnDeathFinished -= HandleOnDeathFinished;
            healthComponent.OnDeathFinished += HandleOnDeathFinished;

            playerInput.actions["Attack"].performed -= OnAttack;
            playerInput.actions["Attack"].performed += OnAttack;
            playerInput.actions["Dodge"].performed -= OnDodge;
            playerInput.actions["Dodge"].performed += OnDodge;
            playerInput.actions["LeftDodge"].performed -= OnLeftDodge;
            playerInput.actions["LeftDodge"].performed += OnLeftDodge;
            playerInput.actions["SelectSlotOne"].performed -= OnWeaponOneSelected;
            playerInput.actions["SelectSlotOne"].performed += OnWeaponOneSelected;
            playerInput.actions["SelectSlotTwo"].performed -= OnWeaponTwoSelected;
            playerInput.actions["SelectSlotTwo"].performed += OnWeaponTwoSelected;

            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled -= OnMove;
            playerInput.actions["Move"].canceled += OnMove;

            playerInput.actions["Look"].performed -= OnLook;
            playerInput.actions["Look"].performed += OnLook;
            playerInput.actions["Look"].canceled -= OnLook;
            playerInput.actions["Look"].canceled += OnLook;
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

        public int GetMaxAttackers()
        {
            return 1;
        }

        private void HandleOnDamageTaken(Vector3 direction, int newHealth)
        {
            animComponent.SetHitParameter(true);
            moveComponent.InterupDodge();
        }

        private void HandleOnDeathStarted(Vector3 hitDirection)
        {
            GameEventsEmitter.EmitEvent(EventType.PlayerDefeated, new GenericEventData { Type = EventType.PlayerDefeated });
        }

        private void HandleOnDeathFinished()
        {
            OnHealthReachedZero?.Invoke();
        }

        private void OnAttack(InputAction.CallbackContext context)
        {
            hasAttacked = context.action.IsPressed();
            projectileComponent.Fire(hasAttacked);
            animComponent.SetFiring(hasAttacked);
        }

        private void OnWeaponOneSelected(InputAction.CallbackContext context)
        {
            projectileComponent.ChangeWeapon(WeaponName.BLASTER);
        }

        private void OnWeaponTwoSelected(InputAction.CallbackContext context)
        {
            projectileComponent.ChangeWeapon(WeaponName.WAVE_BEAM);
        }

        private void OnDodge(InputAction.CallbackContext context)
        {
            Debug.Log("Dodge Right");
            hasDodged = context.action.IsPressed();
            moveComponent.ApplyDogde(hasDodged, DodgeDirection.Right);
        }

        private void OnLeftDodge(InputAction.CallbackContext context)
        {
            Debug.Log("Dodge Left");
            hasDodged = context.action.IsPressed();
            moveComponent.ApplyDogde(hasDodged, DodgeDirection.Left);
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            inputVector = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            lookVector = context.ReadValue<Vector2>();
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
    }
}
