using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

/* ---------- Verlaufseintrag  (Driver → LLM) ------------------ */
[Serializable] public struct MemoryEntry
{
    public float   speed;         
    public float[] target;         
    public float   radius;         
    public float[] radius_center; 
    public float[] npcPos;        
    public float   timestamp;      
}

/* ---------- Antwortstruktur  (LLM → Driver) ------------------ */
[Serializable] public struct AgentAnswer
{
    public float   speed;      
    public float[] target;     
}

/* ---------- Hilfsstrukturen zum Auspacken der Chat‑Antwort --- */
[Serializable] class ChatMessage { public string role; public string content; }
[Serializable] class Choice      { public int index; public ChatMessage message; }
[Serializable] class ChatResp    { public Choice[] choices; }

/* ---------- Request‑Objekte ---------------------------------- */
[Serializable] class RespFmt  { public string type = "json_object"; }
[Serializable] class ChatMsg  { public string role; public string content; public ChatMsg(string r,string c){role=r;content=c;} }
[Serializable] class ChatReq
{
    public string   model = "gpt-4o-mini";
    public float    temperature = .7f;
    public int      max_tokens  = 1000;
    public RespFmt  response_format = new RespFmt();
    public ChatMsg[] messages;
}

public static class LlmBrain
{
    const string ENDPOINT = "https://api.openai.com/v1/chat/completions";
    static string apiKey;
    static string prompt;

    public static void Init(TextAsset key, TextAsset promptText)
    {
        apiKey = key.text.Trim();
        prompt = promptText.text.Trim();
    }

    public static async Task<AgentAnswer> QueryAsync(string userPromptJson)
    {
        // Request‐JSON 
        var reqObj = new ChatReq {
            messages = new [] {
                new ChatMsg("system", prompt),
                new ChatMsg("user",   userPromptJson)
            }
        };
        string body = JsonUtility.ToJson(reqObj, true);
        Debug.Log("[LlmBrain] ► Anfrage an LLM:\n" + body);
    
        using var web = new UnityWebRequest(ENDPOINT,"POST"){
            uploadHandler   = new UploadHandlerRaw(
                                System.Text.Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer() };
        web.SetRequestHeader("Content-Type","application/json");
        web.SetRequestHeader("Authorization","Bearer "+apiKey);

        var op = web.SendWebRequest(); while(!op.isDone) await Task.Yield();

        if (web.result != UnityWebRequest.Result.Success)
            throw new Exception($"OpenAI {web.responseCode}: {web.error}\n{web.downloadHandler.text}");

        // 1) äußere Hülle
        string rawOuter = web.downloadHandler.text;

        ChatResp outer = JsonUtility.FromJson<ChatResp>(rawOuter);
        if (outer.choices == null || outer.choices.Length == 0)
            throw new Exception("[LlmBrain] choices[] leer!");

        string content = outer.choices[0].message.content;

        // 2) eingebettetes JSON ent‑escapen
        string inner = content.Trim();
        if (inner.StartsWith("\"") && inner.EndsWith("\""))
            inner = inner.Substring(1, inner.Length - 2);
        inner = inner.Replace("\\\"", "\"");

        // 3) endgültig in AgentAnswer parsen 
        AgentAnswer ans = JsonUtility.FromJson<AgentAnswer>(inner);
        Debug.Log($"[LlmBrain] ✓ Parsed  speed={ans.speed:F2}  target=({ans.target[0]}, {ans.target[1]})");
        return ans;
    }
}
