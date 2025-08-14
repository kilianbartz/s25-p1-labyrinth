// NPCStateSender.cs
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class NPCStateSender : MonoBehaviour
{
    [System.Serializable]
    public class NPCRequest { public string message; }

    [System.Serializable]
    public class NPCResponse
    {
        public string reply;
        public string reaction;
    }

    public IEnumerator SendMessageToAPI(string message, Action<string> onReactionReceived)
    {
        NPCRequest req = new NPCRequest { message = message };
        string json = JsonUtility.ToJson(req);

        using (UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:5050/chat", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string result = request.downloadHandler.text;
                NPCResponse response = JsonUtility.FromJson<NPCResponse>(result);

                Debug.Log("KI-Antwort: " + response.reply + " | Reaktion: " + response.reaction);

                // Rückgabe der Reaktion an das aufrufende Skript
                onReactionReceived?.Invoke(response.reaction);
            }
            else
            {
                Debug.LogError("API-Fehler: " + request.error);
            }
        }
    }
}
