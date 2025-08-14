using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

[System.Serializable]
public class PromptPayload
{
    public float         radius;         
    public float[]       radius_center;  
    public float[]       npcPos;        
    public MemoryEntry[] history;       
}

/* ------------------------------------------------------------
 * NPC‑Driver, der die LLM‑Antworten in Bewegung umsetzt
 * ---------------------------------------------------------- */
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NpcLlmDriver : MonoBehaviour
{
    public TextAsset openAiKey;
    public TextAsset promptText;
    [Range(1, 30)] public int historyDepth = 10;

    public float baseRadius = 20f;
    public float playerWalkFac = .8f, playerRunFac = .5f;
    public float npcWalkFac = 1.2f, npcRunFac = 1.5f;

    public float minQueryDelay = 0.4f;   // bei Distanz ≈ 0
    public float maxQueryDelay = 5f;     // bei Distanz ≥ baseRadius
    public float maxSpeed = 5f;

    // Runtime 
    NavMeshAgent ag;
    Animator anim;
    Vector3 prevP;                 // Spieler­position aus dem vorherigen Frame (Y=0)
    float radius, distance;
    readonly Queue<MemoryEntry> hist = new();

    public float minCrawlTime = 1f;
    public float maxCrawlTime = 3f;
    private float crawlTime = 0f;
    public float crawlSpeed = 0.5f;
    private float crawlStartTime = 0f;

    void Awake()
    {
        ag = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        LlmBrain.Init(openAiKey, promptText);
        StartCoroutine(QueryLoop());
    }

    void Update()
    {
        var pObj = GameObject.FindWithTag("Player"); if (!pObj) return;
        Vector3 p = pObj.transform.position;
        Vector3 n = transform.position;
        p.y = n.y = 0f;           // nur X‑Z‑Ebene berücksichtigen
        distance = Vector3.Distance(n, p);

        // Radius je Frame neu berechnen 
        radius = Mathf.Max(0, distance) *
                 PlayerFac((p - prevP).magnitude / Mathf.Max(Time.deltaTime, 0.001f)) * //Distanz, die der Spieler seit dem letzten Frame zurückgelegt hat, geteilt durch die Zeit seit dem letzten Frame
                 NpcFac(ag.velocity.magnitude);
        prevP = p;

        anim.SetFloat("speed", ag.velocity.magnitude);
    }

    IEnumerator QueryLoop()
    {
        while (true)
        {
            // ► Aktuelle Welt­daten erfassen
            var pObj = GameObject.FindWithTag("Player");
            Vector3 playerPos3 = pObj ? pObj.transform.position : Vector3.zero;
            Vector3 npcPos3    = transform.position;

            // Prompt‑Payload zusammenstellen
            var payload = new PromptPayload
            {
                radius = R3(radius),
                radius_center = new[] { R3(playerPos3.x), R3(playerPos3.z) },
                npcPos = new[] { R3(npcPos3.x), R3(npcPos3.z) },
                history = hist.ToArray()
            };

            string json = JsonUtility.ToJson(payload, true);

            // LLM‑Call 
            Task<AgentAnswer> task = LlmBrain.QueryAsync(json);
            while (!task.IsCompleted) yield return null;

            if (task.Exception == null)
            {
                var ans = task.Result;

                // Geschwindigkeit & Ziel übernehmen
                float llmSpeed = Mathf.Clamp(ans.speed, 0, maxSpeed);

                if (ans.target == null || ans.target.Length < 2)
                {
                    Debug.LogWarning("LLM lieferte kein gültiges target[2] – NPC wartet");
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                Vector3 llmTarget = new Vector3(ans.target[0], transform.position.y, ans.target[1]);

                // Clamping auf Kreis (nur X‑Z‑Ebene) 
                Vector3 pXZ = new Vector3(playerPos3.x, transform.position.y, playerPos3.z);
                if (Vector3.Distance(pXZ, llmTarget) > radius)
                    llmTarget = pXZ + Vector3.ProjectOnPlane((llmTarget - pXZ), Vector3.up).normalized * radius;

                if (!NavMesh.SamplePosition(llmTarget, out var hit, 2, NavMesh.AllAreas))
                    hit.position = pXZ;

                // Animation für Kriechen und Aufstehen verwalten
                if (crawlTime > 0f && ((Time.time - crawlStartTime) < crawlTime))
                {
                    ag.speed = crawlSpeed;
                    anim.SetFloat("speed", 0f);
                }
                else if (crawlTime > 0f && ((Time.time - crawlStartTime) > crawlTime))
                {
                    anim.SetTrigger("standUp");
                    crawlTime = 0f;
                    anim.SetFloat("speed", 0f);
                    ag.speed = 0f;
                }
                else
                {
                    ag.speed = llmSpeed;
                    anim.SetFloat("speed", llmSpeed);
                }

                ag.SetDestination(hit.position);

                // History‑Eintrag
                if (hist.Count >= historyDepth) hist.Dequeue();

                hist.Enqueue(new MemoryEntry
                {
                    speed = R3(llmSpeed),
                    target = new[] { R3(hit.position.x), R3(hit.position.z) },
                    radius = R3(radius),
                    radius_center = new[] { R3(playerPos3.x), R3(playerPos3.z) },
                    npcPos = new[] { R3(npcPos3.x), R3(npcPos3.z) },
                    timestamp = R3(Time.time)
                });

                Debug.Log($"Radius: {radius:F2}  Center: ({playerPos3.x:F2}/{playerPos3.z:F2})");
            }
            else
            {
                Debug.LogWarning(task.Exception.InnerException?.Message);
            }

            // Delay abhängig von Distanz 
            float t = Mathf.Clamp01(distance / baseRadius); 
            float delay = Mathf.Lerp(minQueryDelay, maxQueryDelay, t);
            yield return new WaitForSeconds(delay);
        }
    }

    // Wird aufgerufen, wenn der NPC mit dem Spieler kollidiert
    void OnTriggerEnter(Collider other)
    {
        if ((other.tag != "Player") || (crawlTime > 0f)) return;
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        Vector3 dir = (contactPoint - transform.position).normalized;
        bool fromFront = Vector3.Dot(transform.forward, dir) > 0f;
        if (fromFront)
        {
            Debug.Log("Game Over!");
            Time.timeScale = 0f;
        }
        else
        {
            anim.SetFloat("speed", 0f);
            ag.speed = 0f;
            anim.SetTrigger("knock");
            crawlTime = Random.Range(minCrawlTime, maxCrawlTime);
        }
    }

    // Wird aufgerufen, wenn die Stolperanimation abgeschlossen ist
    void tripOver()
    {
        Debug.Log("Trip Over!");
        crawlStartTime = Time.time;
    }

    // Wird aufgerufen, wenn die Aufstehanimation abgeschlossen ist
    void standUpOver()
    {
        anim.SetFloat("speed", 0f);
        Debug.Log("Stand Up!");
    }

    // Hilfs‑ & Faktor‑Funktionen
    static float R3(float v) => Mathf.Round(v * 1000f) * 0.001f;   // Rundet auf 3 Nachkommastellen

    // Bestimmt den Faktor je nach Geschwindigkeit, wie sich der Suchradius des NPCs verändert
    float PlayerFac(float v) => v < 0.1f ? 1f : (v < 1.5f ? playerWalkFac : playerRunFac);
    float NpcFac(float v)    => v < 0.1f ? 1f : (v < 1.5f ? npcWalkFac    : npcRunFac);
}
