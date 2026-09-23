using UnityEngine;

public class MenuCameraPan : MonoBehaviour
{
    public float rotationSpeed = 2f; // degrees per second

    [Header("Yaw Bounds")]
    public float minYaw = -90f;
    public float maxYaw = -23f;

    float currentYaw;
    int direction = 1; // 1 = toward maxYaw, -1 = toward minYaw
    float startX;
    float startZ;

    private void Start()
    {
        // preserve whatever tilt/roll you set up in the Inspector
        startX = transform.eulerAngles.x;
        startZ = transform.eulerAngles.z;

        // start at whichever bound is closer to the camera's current Y rotation
        currentYaw = Mathf.Clamp(NormalizeAngle(transform.eulerAngles.y), minYaw, maxYaw);
    }

    private void Update()
    {
        currentYaw += direction * rotationSpeed * Time.deltaTime;

        if (currentYaw >= maxYaw)
        {
            currentYaw = maxYaw;
            direction = -1;
        }
        else if (currentYaw <= minYaw)
        {
            currentYaw = minYaw;
            direction = 1;
        }

        transform.eulerAngles = new Vector3(startX, currentYaw, startZ);
    }

    private float NormalizeAngle(float angle)
    {
        // converts Unity's 0-360 eulerAngles.y into a -180 to 180 range
        // so it lines up correctly with negative bounds like -90/-23
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}