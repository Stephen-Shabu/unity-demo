using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Samples
{
    public class CharactorController : MonoBehaviour, Attackable
    {
        [SerializeField] private Vector2 inputVector = Vector2.zero;
        [SerializeField] private Vector2 lookVector = Vector2.zero;
        [SerializeField] private AnimationComponent animComponent;
        [SerializeField] private CharacterMovement moveComponent;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private HealthFXComponent healthFxComponent;
        [SerializeField] private BaseCameraComponent cameraComponent;
        [SerializeField] private ProjectileComponent projectileComponent;
        [SerializeField] private MeleeComponent meleeComponent;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private bool hasJumped = false;
        [SerializeField] private bool hasAttacked;
        [SerializeField] private bool hasDashed;
        [SerializeField] private bool hasDodged;
        [SerializeField] private bool isHit = false;

        private PlayerInput playerInput;
        private float reactTimer = 0f;
        private Vector3 hitAxis = Vector3.zero;
        private float aimDuration = 0.5f;
        private float aimTime = 0f;
        private bool launchedMeleeAttack = false;
        private PlayerContext playerContext;
        private PlayerStateMachine movementFSM;
        private PlayerStateMachine actionFSM;
        private PlayerStateMachine conditionFSM;

        public void Initialise(BaseCameraComponent camera, PlayerInput input)
        {
            playerInput = input;
            cameraComponent = camera;
            moveComponent.Intialise();
            projectileComponent.ChangeWeapon(WeaponName.BLASTER);
            healthComponent.Initialise();
            healthFxComponent.Initialise();

            playerContext = new PlayerContext
            {
                Rigidbody = rb,
                Transform = transform,
                PlayerInput = playerInput,
                AnimComponent = animComponent,
                MoveComponent = moveComponent,
                HealthComponent = healthComponent,
                HealthFxComponent = healthFxComponent,
                CameraComponent = cameraComponent,
                ProjectileComponent = projectileComponent,
                MeleeComponent = meleeComponent
            };

            movementFSM = new PlayerStateMachine(playerContext);
            movementFSM.AddState(new PlayerIdleState(playerContext, movementFSM));
            movementFSM.AddState(new PlayerMoveState(playerContext, movementFSM));
            movementFSM.AddState(new PlayerDodgeState(playerContext, movementFSM));
            movementFSM.ChangeState<PlayerIdleState>();

            actionFSM = new PlayerActionStateMachine(playerContext);
            actionFSM.AddState(new PlayerIdleActionState(playerContext, actionFSM));
            actionFSM.AddState(new PlayerRangedActionState(playerContext, actionFSM));
            actionFSM.AddState(new PlayerMeleeActionState(playerContext, actionFSM));
            actionFSM.ChangeState<PlayerIdleActionState>();

            conditionFSM = new PlayerStateMachine(playerContext);
            conditionFSM.AddState(new PlayerNormalState(playerContext, conditionFSM));
            conditionFSM.AddState(new PlayerHitReactState(playerContext, conditionFSM));
            conditionFSM.AddState(new PlayerDeadState(playerContext, conditionFSM));
            conditionFSM.ChangeState<PlayerNormalState>();    
        }

        public void UpdateController()
        {
            movementFSM.Update();
            actionFSM.Update();
            conditionFSM.Update();
        }

        public int GetMaxAttackers()
        {
            return 1;
        }
    }
}
