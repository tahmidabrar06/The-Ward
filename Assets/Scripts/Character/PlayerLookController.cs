using UnityEngine;
using UnityEngine.InputSystem;

namespace TheWard.Character
{
    /// <summary>
    /// First-person camera look. The player root handles yaw while the child
    /// camera handles pitch, keeping movement level with hospital floors.
    /// </summary>
    public sealed class PlayerLookController : MonoBehaviour
    {
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private Transform cameraTransform;
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
                pitch = cameraTransform.localEulerAngles.x;
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
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || !lockCursor) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
