using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{

    // rotation sensitivity for horizontal movements
    public float sensitivityHor = 9.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.freezeRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Read the current device safely, without falling back to the disabled legacy Input API.
        Mouse mouse = Mouse.current;
        if (mouse == null || Time.timeScale <= 0f) return;

        // Match the old Mouse X axis scale; mouse delta is already per frame.
        transform.Rotate(0, mouse.delta.ReadValue().x * 0.1f * sensitivityHor, 0);
        
    }
}
