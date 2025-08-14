using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using JetBrains.Annotations;

// ============================================================================
// LLM API Interaktion
// ============================================================================
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CoroutineRunner");
                _instance = go.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}
public static class OpenAIQuery
{
    [Serializable]
    public class Message { public string role; public string content; }
    [Serializable]
    public class ChatRequest { public string model; public Message[] messages; public float temperature = 0.7f; }
    [Serializable]
    public class ChatChoice { public Message message; }
    [Serializable]
    public class ChatResponse { public ChatChoice[] choices; }

    public const string LLMAddress = "http://192.168.2.124:11434";
    public const string Model = "gemma3:4b";

    public static IEnumerator SendChatRequest(string userInput, Action<string> onResult)
    {
        var requestData = new ChatRequest
        {
            model = Model,
            messages = new Message[]
            {
                new Message { role = "user", content = userInput }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest($"{LLMAddress}/v1/chat/completions", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Fehler bei Anfrage: {request.error}");
                onResult?.Invoke(null);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                ChatResponse response = JsonUtility.FromJson<ChatResponse>(responseJson);
                string reply = response.choices[0].message.content;
                onResult?.Invoke(reply);
            }
        }
    }
}