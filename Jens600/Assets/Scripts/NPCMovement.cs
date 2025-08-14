using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(NPCStateSender))]
public class NPCMovement : MonoBehaviour
{
    public Transform playerTransform;
    public float checkInterval = 2f;

    private NavMeshAgent agent;
    private Animator animator;
    private NPCStateSender stateSender;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stateSender = GetComponent<NPCStateSender>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        lastPlayerPosition = playerTransform.position;

        // ▶️ Einmaliges Priming
        //yield return StartCoroutine(SendPrimingMessage());

        // ▶️ Dann beginnt die KI-Abfrage
        StartCoroutine(PeriodicCheck());
    }

    IEnumerator PeriodicCheck()
    {
        while (true)
        {
        if (playerTransform == null) yield return null;

        Vector3 playerPos = playerTransform.position;
        float distance = Vector3.Distance(playerPos, transform.position);
        float speed = (playerPos - lastPlayerPosition).magnitude / checkInterval;
        lastPlayerPosition = playerPos;

        string message = $"Spieler {distance:F1}m entfernt, Geschwindigkeit: {speed:F1} m/s";

        // Warte auf Abschluss, bevor neu gestartet wird
        bool awaiting = true;
        StartCoroutine(stateSender.SendMessageToAPI(message, reaction => {
            ReactToMessage(reaction);
            awaiting = false;
        }));

        // Warte bis Antwort da ist + 5 Sekunden Pause
        yield return new WaitUntil(() => !awaiting);
        yield return new WaitForSeconds(5f);  // zusätzlicher Abstand
    }
    }

    /*
    IEnumerator SendPrimingMessage()
    {
        string prompt =
            "Du spielst einen NPC in einem Unity-Spiel. " +
            "Der Spieler bewegt sich durch ein Labyrinth und kommt dir manchmal näher. " +
            "Sofern der Spieler schneller als üblich auf dich zuläuft, kannst du Angst verspüren " +
            "und deine Reaktionen entsprechend anpassen, allerdings kannst auch freundlich " +
            "gegenüber dem Spieler reagieren, sofern du ihn nicht als Bedrohung wahrnimmst. " +
            "Du kannst die selbe Aktion öfter nacheinander ausführen, aber es muss nicht sein. " +
            "Deine Aufgabe ist es, realistisch zu reagieren. " +
            "Im folgenden eine Liste mit Aktionen die dir zur Auswahl stehen mit den " +
            "entsprechenden Strings, die du zurückgeben musst. Nichts tun und stehen: idle, " +
            " winken: wave, weglaufen: run_away, sich ducken: crouch, rückwärts krabbeln: " +
            "crawl_backwards, traurig herumstehen: sad, glücklich herumstehen: happy. " +
            "Wichtig: Du darfst in allen Antworten, die du ab jetzt gibst nur die vorher " +
            "beschriebenen Wörter verwenden und keine anderen Wörter oder Zeichen." +
            "Merke dir diese Regeln die ganze Zeit und beziehe sie in alle zukünftigen Antworten ein. " +
            "Als Beispiel: Spieler 3.4m entfernt, Geschwindigkeit: 0.0 m/s, deine Antwort darauf wäre: " +
            "happy.";

        yield return StartCoroutine(stateSender.SendMessageToAPI(prompt, reaction => {
            Debug.Log("📌 Priming abgeschlossen. Reaktion: " + reaction);
        }));
    }
    */


    void ReactToMessage(string reaction)
    {
        Debug.Log($"🧠 Reaktion empfangen: {reaction}");

        switch (reaction)
        {
            case "idle":
                animator.SetTrigger("Idle");
                break;

            case "wave":
                animator.SetTrigger("Wave");
                break;

            case "run_away":
                animator.SetTrigger("RunAway");
                Vector3 backward = transform.position - transform.forward * 10f;
                if (agent.SetDestination(backward))
                    Debug.Log("✅ Ziel gesetzt: " + backward);
                else
                    Debug.LogWarning("⚠️ Ziel konnte nicht gesetzt werden");
                break;

            case "crawl_backwards":
                animator.SetTrigger("CrawlBackwards");
                agent.SetDestination(transform.position - transform.forward * 2f);
                break;

            case "crouch":
                animator.SetTrigger("Crouch");
                break;

            case "sad":
                animator.SetTrigger("Sad");
                break;

            case "happy":
                animator.SetTrigger("Happy");
                break;

            default:
                Debug.Log("❔ Unbekannte Reaktion: " + reaction);
                break;
        }
    }
}
