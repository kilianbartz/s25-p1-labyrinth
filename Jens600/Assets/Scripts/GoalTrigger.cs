using UnityEngine;
using UnityEngine.UI;

public class GoalTrigger : MonoBehaviour
{
    public GameObject winUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false); // blendet es einfach aus
            
            if (winUI != null)
                winUI.SetActive(true);

            Debug.Log("Ziel erreicht!");
        }
    }
}
