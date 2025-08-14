# Integration von Ollama/LLMs für realistische(re) Simulationen in der Godot Engine 4.5

Dieses Projekt zeigt, wie Large Language Models (LLMs) mithilfe der Plattform **Ollama** in ein C#-basiertes Godot-4.5-Spielprojekt integriert werden können, um NPCs mit realistischen Reaktionen auf Sicht- und Hörinformationen auszustatten.

---

## 1. Installation von Ollama

### 1.1 Via Docker

1. [Docker installieren](https://docs.docker.com/get-docker/)
2. Image laden:  
   `docker pull ollama/ollama`
3. Container starten:  
   `docker run -d -p 11434:11434 --name ollama ollama/ollama`
4. Modelle installieren:
   ```bash
   curl http://localhost:11434/api/pull -d '{"name": "phi"}'
   curl http://localhost:11434/api/pull -d '{"name": "gemma"}'
   curl http://localhost:11434/api/pull -d '{"name": "mistral"}'
   curl http://localhost:11434/api/pull -d '{"name": "llama3"}'
   curl http://localhost:11434/api/pull -d '{"name": "codellama"}'
   ```
5. Test via CURL:
   `curl http://localhost:11434/api/generate -d '{"model": "llama3", "prompt": "Hello"}`


### 1.2 Via Windows
Installer von https://ollama.com/download herunterladen und ausführen

Modelle wie oben via PowerShell oder CURL installieren

Testbeispiel:
```powershell
Invoke-WebRequest -Uri http://localhost:11434/api/generate -Method POST -Body '{"model": "phi", "prompt": "Hello"}' -ContentType "application/json"
```

## 2. Godot Engine 4.5-Beta2 Projekt mit C#/.NET
Dieses Projekt ist die Fortführung von Aufgabe 1 aus der
```
https://github.com/syssoft-games/s25-p1-labyrinth/
```

Das Projekt basiert auf Godot 4.5 Beta2 mit .NET/C# 
Es erweitert das originale Projekt um ein animiertes modulares NPC-System mit:
- einen NPC mit Sinneswahrnehmung (Augen, Ohren) 
- einem NpcBrainAI (als Gehirn für den NPC)
- einem Ollama/LlmClient (als Anbindung an das LLM)

Zusätzlich wird der Spieler erweitert um:
- einen aktivieren Zustand was er gerade macht
- der Fähigkeit zu Gehen, Sprinten und Schleichen (Animationen für Bewegungsarten)

## 3. C# Anbindung an Ollama API
Die Klasse LlmClient sendet strukturierte Prompts an Ollama (lokal) und wertet Antworten aus. Das Modell entscheidet, wie der NPC reagiert (z. B. laufen, winken, schleichen).
```cs
public async Task<string> SendRequestAsync(string prompt, string model = "llama3", bool stream = false)
{
    var _httpClient = new HttpClient() {
        BaseAddress = new Uri($"http://localhost:11434/api/generate")
    }
    var requestBody = new { model = model, stream = stream, prompt = prompt };
    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("", content);
    return await response.Content.ReadAsStringAsync();
}
```
Modelle: llama3, mistral, phi, gemma, codellama

## 🧩 4. Neue Klassen und Datenstrukturen
### 4.1 Augen und Ohren für NPCs
`Character3D.cs`
```cs
[Export] public Camera3D Eyes;         // Kamera als Sichtorgan
[Export] public EarsArea3D Ears;       // 3D-Area zur Erkennung von Geräuschen
```

`EarsArea3D.cs`
Trackt, ob Spieler/NPCs im Hörbereich sind:
```cs
    public bool IsHearingAnyPlayer()
    {
        return PlayersInRange.Count > 0 && 
               PlayersInRange[0].TryGetAbility<Character3dMovementAbility>(out var ability) &&
               ability.IsHearable();
    }
    public bool IsHearingAnyNpc()
    {
        return PlayersInRange.Count > 0 && 
               PlayersInRange[0].TryGetAbility<Character3dMovementAbility>(out var ability) &&
               ability.IsHearable();
    }
    public bool IsHearingAny()
    {
        return IsHearingAnyPlayer() || IsHearingAnyNpc();
    }
```

### 4.2 Enums zur Steuerung von Persönlichkeiten und Zuständen
```cs
public enum Personality { Neutral, Cautious, Aggressive }
public enum CharacterState { Idle, WaveL, Walk, Run, Sneak }
```

### 4.3 NPC-Zustandsbeschreibung: 
`NpcState.cs`
```cs
public class NpcState
{
    public bool SeesPlayer { get; set; }
    public float VisualDistanceToPlayer { get; set; }
    public double InEyesSightDuration { get; set; }
    public double TimeSinceLastSeen { get; set; }
    
    public bool HearsPlayer { get; set; }
    public float AuditoryDistanceToPlayer { get; set; }
    public double InEarsRangeDuration { get; set; }
    public double TimeSinceLastHeardPlayer { get; set; }
    
    public CharacterState PlayerState { get; set; }
    //When Visible: Idle, WaveL, Walk, Run, Sneak, 
    //When Hearable: ???   ??? , Walk, Run,   X
}
```

### 4.4 Reaktion eines NPCs
`NpcReaction.cs`
```cs
public class NpcReaction {
    public CharacterState Reaction { get; set; }
}
```


### 🤖 4.5 LLM Anbindung & Verarbeitung
LlmClient.cs
Zentrale Klasse für die Kommunikation mit dem LLM:

Prompt-Aufbau:
```cs
var prompt = $"You are the decision brain for a 3D NPC. Given the NPC state... return only {{\"Reaction\": X}}";
```

Senden an API:
```cs
var response = await _httpClient.PostAsync(...);
```

Antwort analysieren & verwenden:
```cs
var npcReaction = ExtractNpcReaction(llmResponse);
ability.UseResponse(npcReaction);
```

### 🧠 4.6 NPC Entscheidungslogik
`NpcBrainAI.cs`
Führt regelmäßig Entscheidungen herbei:
```cs 
public async void UpdateNpcAsync()
{
    while (!Cts.IsCancellationRequested)
    {
    await Task.Delay(2000, Cts.Token);
    var result = await LlmClient.SendNpcBrainRequestAsync(...);
    }
}
```

## 5. Anwendung der LLM-Antwort zur Animation
Die zurückgegebene Reaktion (CharacterState) wird direkt in das Verhalten und die Animation des NPC umgesetzt.
Beispiel: NPC sieht den Spieler → LLM entscheidet "Run" → Animation wird getriggert via ResponseNpcAbility3d.

## 6. Vor- und Nachteile

LLM-Entscheidung vs "Hardcoded" Programmierung

| Vorteil                                                 | Nachteil                                        |
| ------------------------------------------------------- | ----------------------------------------------- |
| ✨ Hochdynamische Reaktionen                             | 🧠 LLMs reagieren nicht deterministisch         |
| 🔁 Reproduzierbarkeit durch feste Seeds/Patches möglich | 💾 RAM/VRAM Bedarf je nach Modell               |
| 🧩 Kombinierbar mit bestehenden State Machines          | 🕗 Antwortzeiten evtl. limitierend für Echtzeit |

## 7. Weitere Alternativen
Es gibt noch 3 weitere Alternativen mit denen man weitere Aspekte testen kann:

### 7.1 Remote-LLMs nutzen (z. B. OpenAI, Gemini)
- Spart RAM/VRAM (verringert Anforderungen)
- Latenz beachten
- DATENSCHUTZ!!! AUFPASSEN WELCHE DATEN GESENDET WERDEN!

### 7.2 Mit Frameworks arbeiten (ML.NET) oder klassische ML-Ansätze
- Eigene "Modelle" bauen durch moderne Frameworks
- Training mit CSV-Daten aus Spielverläufen
- Gut für deterministische Regeln

### 7.3 Eigenes neuronales Netz
- Von Grund auf selbst Programmieren
- Aufwand deutlich höher, aber besser steuerbar

### 8. Fazit
Die Integration von LLMs wie LLaMA 3, CodeLLaMA, Mistral, Gemma, Phi über Ollama bietet eine gute Möglichkeit, NPCs mit einem „entscheidungsfähigen Gehirn“ auszustatten. 
Die Idee an sich ist garnicht verkehrt und würde sicherlich einige Spiele die vor 15-20 Jahren erstellt wurden deutlich aufwerten durch noch dynamischere und unberechenbarere Reaktionen.
Am Ende ist die Frage immer: Welche Daten braucht man? In unserem Beispiel wurden mit Sinnen gearbeitet wie Sehen und Hören.
Vielleicht könnten auch Mögliche Bewegungsrichtungen berücksichtigt werden oder das Zugehören von Fraktionen bzw. Alternativ Charakteristiken: 
Ist ein Charakter eher Schüchtern/Ängstlich? oder doch eher Neutral? oder dir Aggressiv/Feindlich angesehen?
Zu einer guten Simulation gehören sehr viele Parameter die zusmamen komplexe Logiken und Fähigkeiten ergeben.
Ein guter Mix aus "Supervised Learning", "Training" und realistischen Dynamischen Entscheidungen durch gut programmierte Regeln löst meist den Bedarf.


### 9. 3D Charaktere, Design und Animation
![image](https://github.com/user-attachments/assets/bdbd2e94-d633-458c-a874-34fc955c0ca0)
![image](https://github.com/user-attachments/assets/7c0449c8-4429-44af-b93b-ff35d22040c1)
![image](https://github.com/user-attachments/assets/5295ac3f-4622-4ee4-bae6-cfd3751e64c2)
![image](https://github.com/user-attachments/assets/479d2cf9-4970-400a-bbb9-e1261fc0ccda)
![image](https://github.com/user-attachments/assets/91340702-e610-4b8c-b68b-9abb8daafe15)
![image](https://github.com/user-attachments/assets/95a7e540-6d04-4bbd-a1c4-cf4a8259a1de)
![image](https://github.com/user-attachments/assets/0d7077e6-7d9a-4a08-8947-683b2be5f68f)
![image](https://github.com/user-attachments/assets/43ae1413-065c-4fdf-9d4d-720c32d58b9d)
![image](https://github.com/user-attachments/assets/7c20c74c-5d92-4974-87a8-c7f40c15375d)
