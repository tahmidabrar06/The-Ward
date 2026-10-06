using UnityEngine;

public class TriggerElevator : MonoBehaviour
{
    [SerializeField] private OpenElevator[] doors;
    public bool victory = false;
    // OPEN only,
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if(victory)
        {
            foreach (OpenElevator door in doors)
            {
                Debug.Log("Elevator doors opening");
                door.Open();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        foreach (OpenElevator door in doors)
        {
            door.Close();
        }
    }
}