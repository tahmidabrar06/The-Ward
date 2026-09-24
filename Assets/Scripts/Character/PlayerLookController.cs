using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    /// <summary>
    /// The player handles yaw. Mouse look sets the camera's world rotation,
    /// so a head-mounted camera can follow its position without inheriting roll.
    /// </summary>
    public sealed class PlayerLookController : MonoBehaviour
    {
		[SerializeField] public PlayerCharacterController playerController;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform eyeAnchor;
        [SerializeField, Min(0f)] private float sensitivity = 0.12f;
        [SerializeField] private float minimumPitch = -80f;
        [SerializeField] private float maximumPitch = 80f;
        [SerializeField] private bool lockCursor = true;

        private float pitch;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null) cameraTransform = childCamera.transform;
            }

            if (cameraTransform != null)
            {
                pitch = (Quaternion.Inverse(transform.rotation) * cameraTransform.rotation).eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
            }
        }

        private void OnEnable()
        {
            lookAction?.action.Enable();

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            lookAction?.action.Disable();
        }

        private void Update()
        {
            if (lookAction == null || cameraTransform == null) return;

            Vector2 look = lookAction.action.ReadValue<Vector2>() * sensitivity;
            transform.Rotate(Vector3.up, look.x, Space.Self);

            pitch = Mathf.Clamp(pitch - look.y, minimumPitch, maximumPitch);
            ApplyLookRotation();
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;
            if (eyeAnchor != null) 
			{
				float currentOffset = playerController != null ? playerController.CameraCrouchOffset : 0f; // Ugly fix to crouch problem

				cameraTransform.position = eyeAnchor.position - new Vector3(0, currentOffset, 0);
				
			}
            ApplyLookRotation();
        }

        private void ApplyLookRotation()
        {
            cameraTransform.rotation = transform.rotation * Quaternion.Euler(pitch, 0f, 0f);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || !lockCursor) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
