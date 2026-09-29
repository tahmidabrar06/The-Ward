using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{

    // rotation sensitivity
    public float sensitivityVert = 9.0f;

    // pitch -- vertical, min, max
    private float verticalRot = 0;
    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // make the rigid body not change rotation
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.freezeRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // A mouse may be disconnected; also leave the camera still while the game is paused.
        Mouse mouse = Mouse.current;
        if (mouse == null || Time.timeScale <= 0f) return;

        // Match the old Mouse Y axis scale while reading pixel delta from Input System.
        verticalRot -= mouse.delta.ReadValue().y * 0.1f * sensitivityVert;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

        // get current yaw
        float horizontalRot = transform.localEulerAngles.y;

        // set the pitch, yaw remains the same
        transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
    }
}
