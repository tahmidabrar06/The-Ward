using TheWard.Audio;
using TheWard.Combat;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]


public class FirstPerson : MonoBehaviour
{

    public float speed = 6.0f;
    public float gravity = -9.8f;
    private CharacterController charController;

    void Start()
    {
        // get the character controller component
        charController = GetComponent<CharacterController>();
        // Floor3 uses this older controller, so it needs the same music startup as the newer player.
        GameAudio.RegisterPlayer(transform, GetComponent<Health>());
    }

    // User using the WASD keys
    void Update()
    {

        if (Time.timeScale <= 0f) return;
        Vector2 input = Vector2.zero;
        // Use the enabled Input System package; the old Input.GetAxis calls throw in this project.
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        }
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stick = gamepad.leftStick.ReadValue();
            // Use the stronger input instead of adding keyboard and gamepad movement together.
            if (stick.sqrMagnitude > input.sqrMagnitude) input = stick;
        }

        float deltaX = input.x * speed;
        float deltaZ = input.y * speed;

        Vector3 movement = new Vector3(deltaX, 0, deltaZ);

        // make diagonal movement consistent
        movement = Vector3.ClampMagnitude(movement, speed);

        // add gravity in the vertical direction
        movement.y = gravity;

        // ensure movement is independent of the framerate
        movement *= Time.deltaTime;

        // transform from local space to global space
        movement = transform.TransformDirection(movement);

        // pass the movement to the character controller
        charController.Move(movement);
    }
}
