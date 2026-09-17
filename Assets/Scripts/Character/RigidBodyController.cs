using TheWard.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class PlayerRigidbodyController : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference sneakAction;

        [Header("Movement Speeds")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float sneakSpeed = 2.5f;

        [Header("Jumping & Physics")]
        [SerializeField, Min(0f)] private float jumpForce = 5f;
        [SerializeField] private LayerMask groundMask = ~0;
        
        [Header("Sneaking Settings")]
        [SerializeField, Min(0.1f)] private float standingHeight = 2f;
        [SerializeField, Min(0.1f)] private float crouchHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float standingRadius = 0.5f;
        [SerializeField, Min(0.1f)] private float crouchRadius = 0.25f;
        [SerializeField, Min(1f)] private float crouchTransitionSpeed = 10f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask obstacleMask = ~0;

        [Header("Stamina")]
        [SerializeField] private Stamina staminaComponent;

        private Rigidbody rb;
        private CapsuleCollider capsule;
        private bool isCrouching;
        private bool isGrounded;
        private float originalCameraLocalY;
        private float standingCenterY;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            
            standingCenterY = capsule.center.y;
            
            // Prevent the physics engine from tipping the player over
            rb.freezeRotation = true;
            rb.useGravity = true;

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
            GroundCheck();
            HandleJump();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
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
            
            // Apply horizontal movement directly to velocity while preserving gravity (Y axis)
            Vector3 targetVelocity = horizontalMovement * speed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }

        private void HandleJump()
        {
            if (jumpAction != null && jumpAction.action.WasPressedThisFrame() && isGrounded && !isCrouching)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        
        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck() // https://github.com/h33p/Unity-Graphics-Demo/blob/master/Assets/Standard%20Assets/Characters/FirstPersonCharacter/Scripts/RigidbodyFirstPersonController.cs
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo,
                    ((capsule.height/2f) - capsule.radius) + 0.01f, ~0, QueryTriggerInteraction.Ignore))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }

        private void HandleCrouch()
        {
            bool wantsToCrouch = sneakAction != null && sneakAction.action.IsPressed();

            if (!wantsToCrouch && isCrouching)
            {
                if (!CheckIfColliderAbove())
                {
                    isCrouching = false;
                }
            }
            else if (wantsToCrouch)
            {
                isCrouching = true;
            }

            float targetRadius = isCrouching ? crouchRadius : standingRadius;
            float targetHeight = isCrouching ? crouchHeight : standingHeight;

            capsule.height = Mathf.Lerp(capsule.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            capsule.radius = Mathf.Lerp(capsule.radius, targetRadius, Time.deltaTime * crouchTransitionSpeed);
            
            float heightDifference = standingHeight - capsule.height;
            capsule.center = new Vector3(0, standingCenterY - (heightDifference / 2f), 0);

            // Smoothly adjust camera
            if (cameraTransform != null)
            {
                float targetCamHeight = originalCameraLocalY - heightDifference;
                Vector3 camPos = cameraTransform.localPosition;
                camPos.y = Mathf.Lerp(cameraTransform.localPosition.y, targetCamHeight, Time.deltaTime * crouchTransitionSpeed);
                cameraTransform.localPosition = camPos;
            }
        }
        
        private bool CheckIfColliderAbove()
        {
            return
                Physics.Raycast(transform.position, Vector3.up, 1);  //returns true if anything collides with a 1 unit long raycast drawn from player's current position (ultimately checks if something solid is above him)
        }

        private static void EnableAction(InputActionReference actionReference) { actionReference?.action.Enable(); }
        private static void DisableAction(InputActionReference actionReference) { actionReference?.action.Disable(); }
        
        
    }
}