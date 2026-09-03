using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

         // change in pitch
        verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

        // get current yaw
        float horizontalRot = transform.localEulerAngles.y;

        // set the pitch, yaw remains the same
        transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
    }
}
