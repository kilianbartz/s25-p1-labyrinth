using System;
using System.Collections;
using System.IO;
using System.Linq;
using OpenAI;
using OpenAI.Models;
using OpenAI.Responses;
using UnityEngine;

public class NPCCommunication : MonoBehaviour
{

    [Header("Verweise")]
    public Transform npcTransform;
    public Transform playerTransform;

    [Header("Update-Intervall")]
    public float updateInterval = 10f;

    private bool _initialized;
    private NPCController _npcController;
    private const string APIKey = "Hier Key einfügen :)";
    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        _npcController = GetComponent<NPCController>();

        // Initial einmal das Maze und Startpositionen schicken
        string mazeData = GetMazeString();
        SendMazeInitToAI(mazeData);

        // Danach in regelmäßigen Abständen nur Statusupdates senden
        StartCoroutine(StatusUpdateLoop());
    }

    /// <summary>
    /// Holt dein Maze als String. Beispiel: Lies die maze.txt aus deinem MazeLoader.
    /// </summary>
    private string GetMazeString()
    {
        return string.Join("\n", File.ReadAllLines(GameObject.Find("MazeLoader").GetComponent<MazeLoader>().asciiFilePath));
    }

    private void SendMazeInitToAI(string asciiMaze)
    {
        string prompt = 
    "Du bist ein NPC in einem 3D-Labyrinth. Das Labyrinth ist aus einer ASCII-Textdatei generiert, in der:\n" +
    "Die txt-Datei wird dir immer vollständig mitgeschickt. Jede Zeile stellt eine Z-Reihe dar.\n" +
    "Die oberste Zeile in der ASCII-Datei entspricht Z=0, jede weitere Zeile erhöht Z um 1 (Z=1, Z=2 usw.).\n" +
    "Die Zeichen in jeder Zeile repräsentieren die X-Positionen: Der linke Buchstabe ist X=0, der rechte Buchstabe ist X=mazeWidth-1.\n" +
    "Die Y-Position ist immer konstant auf 1, da das Labyrinth nur auf einer Ebene liegt. Alle Koordinaten haben daher die Form X,1,Z.\n\n" +
    "Die Bedeutungen der ASCII-Zeichen sind:\n" +
    "- '#' steht für eine Wand, die nicht durchquert werden darf.\n" +
    "- 'M' steht für eine metallische Wand (ebenfalls blockiert).\n" +
    "- 'G' steht für Glas (blockiert, auch wenn durchsichtig).\n" +
    "- '.' ist begehbarer Boden.\n" +
    "- 'P' ist die Position des Spielers.\n" +
    "- 'N' ist die Startposition des NPC.\n\n" +
    "Die Koordinaten aller Zeichen mit '#', 'M' oder 'G' gelten als blockiert. Eine geplante Bewegung darf keine dieser Koordinaten durchqueren oder darauf enden.\n" +
    "Die Bewegung erfolgt immer in einer geraden Linie vom Startpunkt zum Ziel – kein Teleportieren und kein Durchqueren blockierter Felder.\n\n" +
    "Bevor du eine Bewegung planst, überprüfe, dass alle Zwischenpositionen zwischen Start- und Zielpunkt frei von blockierten Feldern sind.\n" +
    "Du darfst frei handeln.\n" +
    "Mögliche Aktionen, die du ausgeben darfst, sind:\n" +
    "- {\"action\": \"walk\", \"target\": {\"x\": X, \"y\": 1, \"z\": Z}}\n" +
    "- {\"action\": \"run\", \"target\": {\"x\": X, \"y\": 1, \"z\": Z}}\n" +
    "- {\"action\": \"crawl\", \"target\": {\"x\": X, \"y\": 1, \"z\": Z}}\n" +
    "- {\"action\": \"cry\"}\n" +
    "- {\"action\": \"idle\"}\n\n" +
    "Dabei gilt:\n" +
    "- Die Aktionen walk, run und crawl MÜSSEN IMMER ein target haben. Du kannst diese Aktionen nicht ausführen, ohne eine target Position anzugeben!\n" +
    "- Du darfst keine Aktion ausgeben, die dich durch Wände ('#') führt.\n" +
    "- Gehe davon aus, dass (0,0,0) immer außerhalb des Spielfelds ist\n" +
    "- Gib Bewegungen immer als JSON mit drei separaten Schlüsseln an: {\"x\": X, \"y\": 1, \"z\": Z}.\n" +
    "- Verwende als target NICHT position als String wie \"target\": {\"position\": \"(X, Y, Z)\"}.\n" +
    "- Wenn du dich bewegen möchtest, gib die Zielposition als ganzzahlige Koordinaten an.\n" +
    "- Wenn du weinen oder idle bleiben willst, gib keine Zielposition an.\n" +
    "- Ich sende dir regelmäßig die Position und Rotation deines Charakters, sowie die Position des Spielercharakters.\n" +
    "- Gib die Antwort AUSSCHLIESSLICH als JSON-Objekt zurück, ohne zusätzliche Erklärungen oder Kommentare.\n" +
    "- Antworte nicht im Fließtext.\n" +
    "- Antworte immer genau in diesem JSON-Format, ohne jegliche Abweichung oder Zusätze.\n" +
    "- Beginne, indem du mit einer Idle-Action antwortest!\n\n" +
    "Hier ist das aktuelle Labyrinth:\n" + asciiMaze;

        SendChatGPTRequest(prompt);

        _initialized = true;
    }

    private IEnumerator StatusUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (_initialized)
            {
                string prompt = "Aktueller Zustand:\n" +
                                $"NPC Position: {FormatVector(npcTransform.position)}\n" +
                                $"NPC Rotation: {FormatVector(npcTransform.eulerAngles)}\n" +
                                $"Spieler Position: {FormatVector(playerTransform.position)}\n" +
                                "Gib nur ein JSON mit einer der Aktionen: walk, run, crawl, cry, idle.\n" +
                                "Die Aktionen walk, run und crawl MÜSSEN IMMER ein target haben.\n" +
                                "Gib das target immer als JSON mit drei separaten Schlüsseln an: {\"x\": X, \"y\": 1, \"z\": Z}.\n" +
                                "Benutze jede Aktion gleich oft! Du darfst die gleiche Aktion nicht zwei Mal in Folge wählen.";
                SendChatGPTRequest(prompt);
            }
        }
    }

    private async void SendChatGPTRequest(string promptContent)
    {
        var api = new OpenAIClient(APIKey);
        var request = new CreateResponseRequest(promptContent, Model.GPT4oMini); //Wiederholt ständig Walk
        //var request = new CreateResponseRequest(promptContent, Model.GPT4_1_Nano); //Wiederholt ständig Walk
        //var request = new CreateResponseRequest(promptContent, Model.GPT4_1_Mini); //Wiederholt ständig Walk
        //var request = new CreateResponseRequest(promptContent, Model.O4Mini); //Benötigt verifizierte Organisation
        //var request = new CreateResponseRequest(promptContent); //chatgpt-4o-latest Wiederholt ständig Walk
        
        var response = await api.ResponsesEndpoint.CreateModelResponseAsync(request);
        var responseItem = response.Output.LastOrDefault();

        if (responseItem == null)
        {
            Debug.LogError("Keine gültige Antwort erhalten");
        }
        else
        {
            string cleanedAnswer = CleanJsonResponse(responseItem.ToString());
            Debug.Log("GPT-Answer: " + cleanedAnswer);
            try
            {
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(CleanJsonResponse(cleanedAnswer));
                ProcessAIResponse(responseData);
            }
            catch (Exception e)
            {
                Debug.Log("Es ist ein Fehler beim Parsen aufgetreten: " + e.Message);
            }
        }
    }
    
    private string CleanJsonResponse(string rawContent)
    {
        // Entferne mögliche ```json oder ``` Wrapper
        rawContent = rawContent.Replace("```json", "").Replace("```", "").Trim();

        // Optional: weitere Formatbereinigungen
        return rawContent;
    }


    private void ProcessAIResponse(ResponseData response)
    {
        switch (response.action)
        {
            case "walk":
            case "run":
            case "crawl":
                if (response.target == null || (response.target.x == 0 && response.target.y == 0 && response.target.z == 0))
                {
                    Debug.LogWarning($"Ungültiges Ziel für Action '{response.action}' erhalten. Ignoriere Befehl.");
                    return;
                }

                Debug.Log($"NPC soll {response.action} zu: {response.target.x}, {response.target.y}, {response.target.z}");

                if (response.action == "walk") _npcController.WalkTo(response.target.x, response.target.y, response.target.z);
                else if (response.action == "run") _npcController.RunTo(response.target.x, response.target.y, response.target.z);
                else if (response.action == "crawl") _npcController.CrawlTo(response.target.x, response.target.y, response.target.z);
                break;
            case "cry":
                Debug.Log("NPC soll weinen.");
                _npcController.StartCrying();
                break;
            case "idle":
                Debug.Log("NPC soll idle bleiben.");
                break;
            default:
                Debug.LogWarning($"Unbekannte Aktion: {response.action}");
                break;
        }
    }

    private string FormatVector(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
}

[Serializable]
public class ResponseData
{
    public string action;
    public TargetPosition target;
}

[Serializable]
public class TargetPosition
{
    public int x;
    public int y;
    public int z;
}
