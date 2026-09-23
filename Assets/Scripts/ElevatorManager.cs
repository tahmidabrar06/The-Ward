using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorManager : MonoBehaviour
{
    [SerializeField] private string nextFloorScene = "Floor2";

    private bool hasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasEntered)
            return;

        if (other.CompareTag("Player"))
        {
            hasEntered = true;
            GoToNextFloor();
        }
    }

    private void GoToNextFloor()
    {
        SceneManager.LoadScene(nextFloorScene);
    }
}