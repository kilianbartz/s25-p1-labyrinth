using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using LabyrinthExplorer3D.scripts.game.ai.data;
using LabyrinthExplorer3D.scripts.game.components;
using LabyrinthExplorer3D.scripts.game.npc;
using LabyrinthExplorer3D.scripts.game.npc.ability;
using LabyrinthExplorer3D.scripts.game.npc.behaviours;
using HttpClient = System.Net.Http.HttpClient;

namespace LabyrinthExplorer3D.scripts.game.ai;

[GlobalClass]
public partial class LlmClient : Node
{
    public enum Model
    {
        llama3,
        gemma,
        mistral,
        phi,
        codellama
    }
    
    [Export] public Model OllamaModel = Model.llama3;
    [Export] public bool UseStream = false;
    

    public static NpcReaction ExtractNpcReaction(string llmFullResponse)
    {
        try
        {
            // Step 1: Read the Response JSON from the LLM
            using var doc = JsonDocument.Parse(llmFullResponse);

            // Step 2: Get the "response" JSON
            if (!doc.RootElement.TryGetProperty("response", out var responseElement))
                return null;
            var reactionJson = responseElement.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(reactionJson))
                return null;
            
            // Step 3: Try to parse the json-string into a NpcReaction 
            // NOTE: Sometimes illegal characters, whitespaces or \n and such are returned
            var npcReaction = JsonSerializer.Deserialize<NpcReaction>(reactionJson);
            return npcReaction;
        }
        catch (Exception ex)
        {
            // If any errors occur
            GD.PrintErr("NpcReaction Parsing failed: ", ex.Message);
            return null;
        }
    }
    
    public async Task<string> SendRequestAsync(string prompt)
    {
        HttpClient _httpClient = new HttpClient() {
            BaseAddress = new Uri($"http://localhost:11434/api/generate")
        };
        
        var requestBody = new
        {
            model = OllamaModel.ToString(),
            stream = UseStream,
            prompt = prompt
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );
        
        using var response = await _httpClient.PostAsync("", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<NpcReaction> SendNpcBrainRequestAsync(Npc3D npc, Player3D player)
    {
        var canGetEarsBhvr = npc.TryGetBehaviour<EarsNpcBehaviour3D>(out var earsBhvr);
        var canGetEyesBhvr = npc.TryGetBehaviour<EyesNpcBehaviour3D>(out var eyesBhvr);
        var canGetState = player.TryGetComponent<CharacterStateComponent>(out var stateComp);
        if (!canGetEarsBhvr || !canGetEyesBhvr || !canGetState)
        {
            GD.Print(canGetEarsBhvr, canGetEyesBhvr, canGetState);
            return null;
        }

        var npcState = new NpcState()
        {
            SeesPlayer = eyesBhvr.SeesPlayer,
            VisualDistanceToPlayer = eyesBhvr.DistanceToPlayer,
            InEyesSightDuration = eyesBhvr.TimeInSight,
            TimeSinceLastSeen = eyesBhvr.TimeSinceLastSeen,

            HearsPlayer = earsBhvr.HearsPlayer,
            AuditoryDistanceToPlayer = earsBhvr.DistanceToPlayer,
            InEarsRangeDuration = earsBhvr.TimeWithAudio,
            TimeSinceLastHeardPlayer = earsBhvr.TimeSinceLastAudio,

            PlayerState = stateComp.CharacterState
        };
        var npcStateJson = JsonSerializer.Serialize(npcState);

        // Enumwerte und Beispiele für den Prompt
        var allowedReactions = new Dictionary<CharacterState, int>
        {
            { CharacterState.Idle, 0 },
            { CharacterState.WaveL, 1 },
            { CharacterState.Walk, 2 },
            { CharacterState.Run, 3 },
            { CharacterState.Sneak, 4 }
        };
        var allowedReactionsJson = string.Join(", ", allowedReactions.Select(e => $"{e.Value}({e.Key})"));
        var exampleOutputs = string.Join(", ", allowedReactions.Select(e => $"{{\"Reaction\": {e.Value}}}"));

        // Neuer, robuster Prompt
        var prompt = 
$@"You are the decision brain for a 3D NPC. Given the NPC state (as JSON below), return ONLY a valid and realistic JSON response in the form {{""Reaction"": X}}, 
where X is one of the following: {allowedReactionsJson} (numbers only, see enum).
NO explanations, NO extra text, NO comments. Only ONE JSON object.
NPC state: {npcStateJson}
Example outputs: {exampleOutputs}";

        var llmResponse =  await SendRequestAsync(prompt: prompt);
        
        var npcReaction = ExtractNpcReaction(llmResponse);
        if (npcReaction == null)
            return null;
        
        var currentNpc = NpcController3D.Instance.CurrentNpc;
        var canGet = currentNpc.TryGetAbility<ResponseNpcAbility3d>(out var ability);
        if (!canGet)
            return null;
        
        ability.UseResponse(npcReaction);
        
        return npcReaction;
    }
}