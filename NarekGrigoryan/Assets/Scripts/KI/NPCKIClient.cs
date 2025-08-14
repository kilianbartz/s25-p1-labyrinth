using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using System.Text.RegularExpressions;

#region Mini-DTOs
[System.Serializable] public class ChatResponse { public Choice[] choices; }
[System.Serializable] public class Choice       { public Message message; }
[System.Serializable] public class Message      { public string content; }
#endregion

public class NPCKIClient : MonoBehaviour
{
    /* ─────────────────────────── Server-Setup ─────────────────────────── */
    [Header("LM-Studio-Server")]
    [SerializeField] private string apiUrl      = "http://136.199.51.131:1234/v1/chat/completions";
    [SerializeField] private string bearerToken = "lm-studio";
    [SerializeField] private string modelName   = "deepseek-coder-v2-lite-instruct";

    /* ─────────────────────────── Referenzen ───────────────────────────── */
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator  animator;
    [SerializeField] private TextMeshProUGUI debugText;

    /* ─────────────────────────── Timings ──────────────────────────────── */
    [Header("Timings")]
    [SerializeField] private float sendInterval = 1f;

    /* ─────────────────────────── KI-Prompt ────────────────────────────── */
    /* NEU: Mode 4 + Kontextbeschreibung */
    const string systemPrompt =
        "Du steuerst das Verhalten eines NPCs in Unity. " +
        "Entscheide anhand des JSON-Kontexts eine einzige Ziffer 0–4 zurückzugeben " +
        "(0=Idle, 1=Walk, 2=Run, 3=Crawl, 4=Hide). " +
        "Keine weiteren Zeichen, nur die Ziffer.";

    /* ─────────────────────────── Laufzeit-Cache ───────────────────────── */
    int   lastMode         = -1;
    Vector3 lastPlayerPos;
    float lastSampleTime;

    /* FOV-Schwellwert, ab wann der Spieler den NPC „anschaut“ */
    [Header("Look Detection")]
    [SerializeField] private float lookDotThreshold = 0.7f;

    /* ─────────────────────────── Unity Hooks ──────────────────────────── */
    void Awake()
    {
        if (!player)   player   = GameObject.FindWithTag("Player")?.transform;
        if (!animator) animator = GetComponent<Animator>();

        lastPlayerPos   = player ? player.position : Vector3.zero;
        lastSampleTime  = Time.time;
    }

    IEnumerator Start()
    {
        while (true)
        {
            yield return SendStateAndHandleReply();
            yield return new WaitForSeconds(sendInterval);
        }
    }

    /* ─────────────────────────── Haupt-Routine ────────────────────────── */
    IEnumerator SendStateAndHandleReply()
    {
        /* ---------- Kontext ermitteln (NEU) ---------- */
        float dist = player ? Vector3.Distance(transform.position, player.position) : 0f;

        float dt   = Time.time - lastSampleTime;
        float speed = (dt > 0f) ? Vector3.Distance(player.position, lastPlayerPos) / dt : 0f;

        lastPlayerPos  = player.position;
        lastSampleTime = Time.time;

        bool playerLookingAtNpc = false;
        if (player)
        {
            Vector3 toNpc = (transform.position - player.position).normalized;
            playerLookingAtNpc = Vector3.Dot(player.forward, toNpc) > lookDotThreshold;
        }

        /* ---------- Kontext in JSON verpacken ---------- */
        var contextObj = new
        {
            distanceToPlayer     = dist,
            playerSpeed          = speed,
            playerLookingAtNpc   = playerLookingAtNpc,
            npcCurrentMode       = lastMode
        };
        string userJson = JsonUtility.ToJson(contextObj);

        string payload =
$@"{{
  ""model"": ""{modelName}"",
  ""messages"": [
    {{ ""role"": ""system"", ""content"": ""{systemPrompt}"" }},
    {{ ""role"": ""user"",   ""content"": ""{userJson}"" }}
  ],
  ""temperature"": 0.4,
  ""max_tokens"": 1,
  ""stream"": false
}}";

        using (var req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {bearerToken}");
#if UNITY_EDITOR
            req.certificateHandler = new BypassCertificate(); // Uni-TLS umgehen
#endif
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string json = req.downloadHandler.text;
                string ans  = string.Empty;

                try
                {
                    ChatResponse resp = JsonUtility.FromJson<ChatResponse>(json);
                    if (resp?.choices?.Length > 0)
                        ans = resp.choices[0].message.content.Trim();
                    ans = Regex.Match(ans, "[0-4]").Value;              // NEU: 0-4
                }
                catch { /* Parsing-Fehler -> Fallback */ }

                if (!int.TryParse(ans, out int mode) || mode == lastMode)
                {
                    mode = Random.Range(0, 5);                          // NEU: 0-4
                    ans  = mode.ToString();
                }

                Debug.Log($"[KI-ANTWORT] {ans}");

                lastMode = mode;
                if (debugText) debugText.text = ans;
                HandleAnswer(mode);
            }
            else
            {
                Debug.LogWarning($"HTTP-Fehler: {req.error}");
                HandleAnswer(-1);
            }
        }
    }

    /* ─────────────────────────── Animator-Übergabe ────────────────────── */
    void HandleAnswer(int mode)
    {
        if (animator && mode >= 0)
        {
            animator.SetInteger("Mode", mode);
            Debug.Log($"[Animator] Mode gesetzt: {mode}");
        }
        else if (animator)
        {
            animator.SetInteger("Mode", -1);
            Debug.LogWarning("[KI] Ungültige Mode-Antwort, führe Idle aus.");
        }
    }

    /* ─────────────────────────── TLS-Bypass nur Editor ────────────────── */
    class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] cert) => true;
    }
}