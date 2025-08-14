using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NpcToAI : MonoBehaviour
{
    [TextArea]
    public string npcState;
    public TMP_Text outputText; // KI-Antwort
    public string apiUrl = "http://localhost:5005/npcstate";
    public bool sended = false;
    public CreatureController creatureController;
    public GameObject player;
    float timer = 0;

    public void Update(){
        if(timer > 5){
            npcState = "Das Pferd kann sich bewegen (moveto), fliehen vor dem Spieler (fleefrom), stoppen (stop) und in die Richtung des Spielers schauen (lookat). Zudem ist es neugierig, aber auch ängstlich. Es steht auf Position: " + creatureController.transform.position + " und der Spieler befindet sich auf Position " + player.transform.position + " Was soll das Pferd jetzt tun? Antworte nur mit einem Wort: move, flee, stop oder look";
            StartCoroutine(SendPromptToAPI(npcState));
            timer = 0;
        }
        timer = timer + Time.deltaTime;

     /*   if((npcState.Contains("\n") || npcState.Contains("\r")) && !sended){
            StartCoroutine(SendPromptToAPI(npcState));
            sended = true;
            Debug.Log(npcState);
        }*/
    }

    IEnumerator SendPromptToAPI(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)){
        Debug.LogError("Prompt ist leer!");
        yield break;
        }

        string json = JsonUtility.ToJson(new PromptData { prompt = prompt });

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

#if UNITY_EDITOR
        // HTTP in Editor erlauben
        request.certificateHandler = new BypassCertificate(); 
#endif

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Fehler beim Senden: " + request.error);
        }
        else
        {
            string response = request.downloadHandler.text;
            string content = JsonUtility.FromJson<ResponseWrapper>(response).response;
            //outputText.text = "KI: " + content;

            // Verhalten basierend auf KI-Antwort steuern
            HandleBehavior(content);
            Debug.Log(content);
        }
        npcState = "";
        sended = false;
    }

    private void HandleBehavior(string command)
{
    command = command.ToLower();

    Vector3 playerPos = player.transform.position;
    Vector3 npcPos = creatureController.transform.position;

    if (command.Contains("move"))
    {
        creatureController.MoveTo(playerPos, run: true);
    }
    else if (command.Contains("flee"))
    {
        creatureController.FleeFrom(playerPos, run: true);
    }
    else if (command.Contains("stop"))
    {
        creatureController.Stop();
    }
    else if (command.Contains("look"))
    {
        creatureController.LookAt(playerPos);
    }
    else
    {
        Debug.LogWarning("Unbekannter KI-Befehl: " + command);
        creatureController.Stop();
    }
}


    [System.Serializable]
    public class ResponseWrapper
    {
        public string response;
    }

    class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificate) => true;
    }

    [System.Serializable]
    public class PromptData
    {
        public string prompt;
    }
}
