using UnityEngine;

public class OpenElevator : MonoBehaviour
{
    public Vector3 openOffset;    
    public float speed = 1.0f;  

    Vector3 closedPosition; 
    Vector3 openPosition;
    bool isMoving = false;
    int direction = -1;
    float interpolate = 0.0f;

    void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (isMoving)
        {
            interpolate += direction * speed * Time.deltaTime;

            if (interpolate > 1)
            {
                interpolate = 1;
                isMoving = false;
            }
            else if (interpolate < 0)
            {
                interpolate = 0;
                isMoving = false;
            }

            transform.localPosition = Vector3.Lerp(closedPosition, openPosition, interpolate);
        }
    }

    public void Open()
    {
        isMoving = true;
        direction = 1;
    }

    public void Close()
    {
        isMoving = true;
        direction = -1;
    }
}
