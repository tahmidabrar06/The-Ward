using TheWard.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference sneakAction;
        
        [Header("Movement Speeds")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float sneakSpeed = 2.5f;
        [SerializeField] private Stamina staminaComponent;
        
        [Header("Jumping")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Sneaking Settings")]
        [SerializeField, Min(0.1f)] private float standingHeight = 2f;
        [SerializeField, Min(0.1f)] private float crouchHeight = 0.75f;
        [SerializeField, Min(1f)] private float crouchTransitionSpeed = 10f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask obstacleMask = ~0;
        
        private CharacterController characterController;
        private float verticalVelocity;
        
        private bool isCrouching;
        private float standingCenterY;
        private float originalCameraLocalY;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            
            standingCenterY = characterController.center.y;
            
            if (cameraTransform == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null) cameraTransform = childCamera.transform;
            }

            if (cameraTransform != null)
            {
                originalCameraLocalY = cameraTransform.localPosition.y;
            }
        }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(jumpAction);
            EnableAction(sprintAction);
            EnableAction(sneakAction);
        }

        private void OnDisable()
        {
            DisableAction(moveAction);
            DisableAction(jumpAction);
            DisableAction(sprintAction);
            DisableAction(sneakAction);
        }
        
        private void Update()
        {
            HandleCrouch();

            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 horizontalMovement = transform.right * input.x + transform.forward * input.y;
            horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, 1f);
            
            bool wantsToSprint = sprintAction != null && sprintAction.action.IsPressed() && !isCrouching && horizontalMovement.sqrMagnitude > 0.1f;
            
            bool isSprinting = false;
            
            if (wantsToSprint && staminaComponent != null && staminaComponent.CanSprint())
            {
                isSprinting = true;
                staminaComponent.DrainStamina();
            }
            else if (wantsToSprint && staminaComponent == null)
            {
                isSprinting = true;
            }

            float speed = isCrouching ? sneakSpeed : (isSprinting ? sprintSpeed : walkSpeed);

            if (characterController.isGrounded)
            {
                verticalVelocity = -2f;
            }

            if (jumpAction != null && jumpAction.action.WasPressedThisFrame() && characterController.isGrounded && !isCrouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = horizontalMovement * speed;
            movement.y = verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }
        
        private void HandleCrouch()
        {
            bool wantsToCrouch = sneakAction != null && sneakAction.action.IsPressed();

            if (wantsToCrouch)
            {
                isCrouching = true;
            }
            else if (isCrouching)
            {
                float radius = characterController.radius;
                Vector3 castOrigin = transform.position + characterController.center;
                float castDistance = standingHeight - characterController.height;
                
                if (!Physics.SphereCast(castOrigin, radius, Vector3.up, out _, castDistance, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    isCrouching = false;
                }
            }
            
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            
            float heightDifference = standingHeight - characterController.height;
            characterController.center = new Vector3(characterController.center.x, standingCenterY - (heightDifference / 2f), characterController.center.z);
            
            if (cameraTransform != null)
            {
                float heightRatio = characterController.height / standingHeight;
                Vector3 camPos = cameraTransform.localPosition;
                camPos.y = Mathf.Lerp(cameraTransform.localPosition.y, originalCameraLocalY * heightRatio, Time.deltaTime * crouchTransitionSpeed);
                cameraTransform.localPosition = camPos;
            }
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
