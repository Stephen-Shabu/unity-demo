using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

namespace Samples
{
    public class CharactorController : MonoBehaviour
    {
        [SerializeField] private Vector2 inputVector = Vector2.zero;
        [SerializeField] private Vector2 lookVector = Vector2.zero;
        [SerializeField] private AnimationComponent animComponent;
        [SerializeField] private CharacterMovement moveComponent;
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

            playerInput.actions["Jump"].performed -= OnJump;
            playerInput.actions["Jump"].performed += OnJump;
        }
        public void OnAttack(InputAction.CallbackContext context)
        {
            hasAttacked = context.action.IsPressed();
            projectileComponent.Fire(hasAttacked);
            animComponent.SetFiring(hasAttacked);
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            Debug.Log("Dodge");
            hasDodged = context.action.IsPressed();
            moveComponent.ApplyDogde(hasDodged);
        }

        public void OnLeftDodge(InputAction.CallbackContext context)
        {
            Debug.Log("Dodge");
            hasDodged = context.action.IsPressed();
            moveComponent.ExecuteLeftDogde(hasDodged);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            inputVector = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            lookVector = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            hasJumped = context.action.IsPressed();
            moveComponent.ApplyJumpVelocity(hasJumped);
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
                playerInput.actions["Jump"].performed -= OnJump;                
            }
        }
    }
}
