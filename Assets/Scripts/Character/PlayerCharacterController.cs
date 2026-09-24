using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float sprintSpeed = 7f;

        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            sprintAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            sprintAction?.action.Disable();
        }

        private void Update()
        {
            Vector2 input = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            Vector3 horizontalMovement = transform.right * input.x + transform.forward * input.y;
            horizontalMovement = Vector3.ClampMagnitude(horizontalMovement, 1f);
            bool sprinting = sprintAction != null && sprintAction.action.IsPressed();
            float speed = sprinting ? sprintSpeed : walkSpeed;

            Vector3 movement = horizontalMovement * speed;
            characterController.Move(movement * Time.deltaTime);
        }
    }
}
