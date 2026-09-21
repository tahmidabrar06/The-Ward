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
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField, Min(0)] private int extraAirJumps = 1;
        [SerializeField] private float gravity = -20f;

        private CharacterController characterController;
        private float verticalVelocity;
        private int airJumpsRemaining;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(jumpAction);
            EnableAction(sprintAction);
        }

        private void OnDisable()
        {
            DisableAction(moveAction);
            DisableAction(jumpAction);
            DisableAction(sprintAction);
        }

        private void Update()
        {
            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 horizontalMovement = transform.right * input.x + transform.forward * input.y;
            horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, 1f);
            bool sprinting = sprintAction != null && sprintAction.action.IsPressed();
            float speed = sprinting ? sprintSpeed : walkSpeed;

            if (characterController.isGrounded)
            {
                // A small downward speed keeps the controller touching the floor.
                verticalVelocity = -2f;
                airJumpsRemaining = extraAirJumps;
            }

            if (jumpAction != null && jumpAction.action.WasPressedThisFrame() &&
                (characterController.isGrounded || airJumpsRemaining > 0))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (!characterController.isGrounded)
                {
                    airJumpsRemaining--;
                }
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = horizontalMovement * speed;
            movement.y = verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
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
