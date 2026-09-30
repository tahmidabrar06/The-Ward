using UnityEngine;

public class RotateDoor : MonoBehaviour
{
    public Vector3 openRotAngle;    // rotation angle when open
    public Vector3 closeRotAngle;   // rotation angle when closed
    public float speed = 1.0f;      // door rotation speed

    bool isRotating = false;        // is the door currently rotating
    int direction = -1;             // rotation direction
    float interpolate = 0.0f;       // interpolation amount

    // check if door is opened
    public bool IsOpen
    {
        get { return direction == 1; }
    }

    void Update()
    {
        // if rotation is required
        if (isRotating)
        {
            interpolate += direction * speed * Time.deltaTime;

            if (interpolate > 1)
            {
                interpolate = 1;
                isRotating = false;
            }
            else if (interpolate < 0)
            {
                interpolate = 0;
                isRotating = false;
            }

            transform.localRotation = Quaternion.Euler(Vector3.Lerp(closeRotAngle, openRotAngle, interpolate));
        }
    }

    // open door
    public void Open()
    {
        Debug.Log("Opened the door");
        isRotating = true;
        direction = 1;
    }

    // close door
    public void Close()
    {
        Debug.Log("Closed the door");
        isRotating = true;
        direction = -1;
    }
}