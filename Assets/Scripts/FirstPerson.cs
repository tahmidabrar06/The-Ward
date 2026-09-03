using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    }

    // User using the WASD keys
    void Update()
    {

        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;

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
