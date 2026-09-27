using TheWard.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference sneakAction;
        
        [Header("Movement Speeds")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float sneakSpeed = 2.5f;
        [SerializeField] private Stamina staminaComponent;

        [Header("Sneaking Settings")]
        //[SerializeField, Min(0.1f)] private float standingHeight = 2f;
        private float standingHeight;
        [SerializeField, Min(0.1f)] private float crouchHeight = 0.5f;
        [SerializeField, Min(1f)] private float crouchTransitionSpeed = 10f;
        public float CameraCrouchOffset { get; private set; }
        private CharacterController characterController;
        private float verticalVelocity;
        private bool isCrouching;
        private float standingCenterY;
        private float originalCameraLocalY;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            standingCenterY = characterController.center.y;
            standingHeight = characterController.height;
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            sprintAction?.action.Enable();
            EnableAction(moveAction);
            EnableAction(sprintAction);
            EnableAction(sneakAction);
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            sprintAction?.action.Disable();
            DisableAction(moveAction);
            DisableAction(sprintAction);
            DisableAction(sneakAction);
        }
        
        private void Update()
        {
            HandleCrouch();

            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 horizontalMovement = transform.right * input.x + transform.forward * input.y;
            horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, 1f);
            
            bool wantsToSprint = (sprintAction != null && sprintAction.action.IsPressed() && !isCrouching && horizontalMovement.sqrMagnitude > 0.1f);
            
            bool isSprinting = false;
            
            if (wantsToSprint && staminaComponent != null && staminaComponent.CanSprint())
            {
                isSprinting = true;
                staminaComponent.DrainStamina();
            }

            float speed = isCrouching ? sneakSpeed : (isSprinting ? sprintSpeed : walkSpeed);
            
            Vector3 movement = horizontalMovement * speed;
            
            if (characterController.isGrounded) verticalVelocity = -2f;
            
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            movement.y = verticalVelocity;
            
            characterController.Move(movement * Time.deltaTime);
        }
        
        private void HandleCrouch()
        {
            bool wantsToCrouch = sneakAction != null && sneakAction.action.IsPressed();

            if (!wantsToCrouch && isCrouching && !CheckIfColliderAbove()) // Wants to stand up
            {
                    isCrouching = false;
            }
            else if (wantsToCrouch)
            {
                isCrouching = true;
            }
            
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            
            float targetCameraOffset = isCrouching ? (standingHeight - crouchHeight) : 0f;
            CameraCrouchOffset = Mathf.Lerp(CameraCrouchOffset, targetCameraOffset, Time.deltaTime * crouchTransitionSpeed);
        }
        
        private bool CheckIfColliderAbove()
        {
            float distance = standingHeight - crouchHeight;
            return Physics.Raycast(transform.position, Vector3.up, distance);  //returns true if anything collides with a 1 unit long raycast drawn from player's current position (ultimately checks if something solid is above him)
        }

        private static void EnableAction(InputActionReference actionReference)
        {
            actionReference?.action.Enable();
        }

        private static void DisableAction(InputActionReference actionReference)
        {
            actionReference?.action.Disable();
        }
    }
}
