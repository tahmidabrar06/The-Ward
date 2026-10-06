using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorManager : MonoBehaviour
{
    [SerializeField] private string nextFloorScene = "Floor2";

    private bool hasEntered = false;

    //temporary, this needs to be called from victory screen, on button press - tahmid
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