using UnityEngine;

public class EndTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // nur einmal

        Debug.Log("DEBUG: Trigger betreten von " + other.name);

        // Optional: prüfe auf Player-Tag
        if (other.CompareTag("Player"))
        {
            Debug.Log("🎉 Maze geschafft!");
            triggered = true;
        }
    }
}