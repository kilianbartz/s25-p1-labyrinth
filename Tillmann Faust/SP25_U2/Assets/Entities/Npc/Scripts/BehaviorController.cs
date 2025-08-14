using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Base-Klasse eines Behavior Controllers. Benutzt Daten (Eigene Position, Position anderer NSCs und Spieler, Terrain) eines StateManagers und manipuliert
/// dessen Zustand anschließend basierend darauf. KI steuerung also.
/// </summary>
public abstract class BaseBehaviorController
{
    protected IControllable npc;
    protected GridWorldInfo worldInfo;
    protected Transform playerTransform;
    public BaseBehaviorController(IControllable npc, GridWorldInfo worldInfo, Transform playerTransform)
    {
        this.npc = npc;
        this.worldInfo = worldInfo;
        this.playerTransform = playerTransform;
    }

    public abstract void Update();
}

/// <summary>
/// Primitive AI behavior that generates a random point on a radius around an IControllable's spawn and setting it's target there.
/// </summary>
public class PrimitiveBehaviorController : BaseBehaviorController
{
    float AllowedRadius;
    float TimeElapsed;
    float BehaviorChangeTime;
    public PrimitiveBehaviorController(
        IControllable npc,
        GridWorldInfo WorldInfo,
        Transform Player,
        float AllowedRadius,
        float BehaviorChangeTime
    ) : base(npc,WorldInfo,Player)
    {
        this.AllowedRadius = AllowedRadius;
        this.BehaviorChangeTime = BehaviorChangeTime;
        TimeElapsed = 0f;
    }

    public override void Update()
    {
        TimeElapsed += Time.deltaTime;
        if (TimeElapsed < BehaviorChangeTime) return;
        Vector2 unitTargetXZ = Random.insideUnitCircle;
        npc.TargetXZ = (unitTargetXZ * AllowedRadius) + npc.SpawnXZ;
        TimeElapsed = 0;
    }
}

public class GridBehaviorController : BaseBehaviorController
{
    public float timeElapsed;
    public float behaviorChangeTime;
    public GridBehaviorController(
        IControllable npc,
        GridWorldInfo worldInfo,
        Transform playerTransform
    ) : base(npc, worldInfo, playerTransform)
    {
        this.behaviorChangeTime = 5f;
        this.timeElapsed = behaviorChangeTime - 0.5f; // Damit der NPC direkt etwas macht
    }

    public override void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed < behaviorChangeTime) return;
        // Suche einen Zufälligen, unblockierten Punkt auf der karte
        int x, y;
        bool isOccupied;
        do
        {
            x = (int)Random.Range(0, worldInfo.WorldSize.x);
            y = (int)Random.Range(0, worldInfo.WorldSize.y);
            isOccupied = worldInfo.WorldLayout[x][y] == "1";
        } while (isOccupied);
        // Setze TargetXZ auf die Koordinaten diesen Punktes.
        npc.TargetXZ = new Vector2(x + 0.5f, y + 0.5f) * worldInfo.WorldScaleFactor;
        // Bestimme ob laufen oder gehen (Wirf einen D20, on 1, RUN!)
        int roll = Random.Range(0, 20);
        if (roll == 0) npc.SetRunning();
        else npc.SetWalking();
        Debug.Log($"Player grid position: {GetPlayerGridPosition()}");
        timeElapsed = 0f;
    }

    (int, int) GetPlayerGridPosition()
    {
        Vector3 playerPos = playerTransform.position;
        Vector2 playerXZ = new Vector2(playerPos.x, playerPos.z);
        int gridX = Mathf.FloorToInt(playerXZ.x / worldInfo.WorldScaleFactor);
        int gridY = Mathf.FloorToInt(playerXZ.y / worldInfo.WorldScaleFactor);

        gridX = Mathf.Clamp(gridX, 0, Mathf.FloorToInt(worldInfo.WorldSize.x) - 1);
        gridY = Mathf.Clamp(gridY, 0, Mathf.FloorToInt(worldInfo.WorldSize.y) - 1);

        return (gridX, gridY);
    }
}

public class LLMGridBehaviorController : BaseBehaviorController
{
    bool currentlyThinking = false;
    string lastLLMResponse = null;
    string coordinatePattern = @"\((\d+),\s*(\d+)\)";   // Regex Audruck zum Extrahieren der Koordinated der Folrm (x,y)
    public LLMGridBehaviorController(
        IControllable npc,
        GridWorldInfo worldInfo,
        Transform playerTransform
    ) : base(npc, worldInfo, playerTransform)
    {
        npc.TargetXZ = npc.SpawnXZ + new Vector2(1f,1f); // Sete eine Default Position
        currentlyThinking = true;
        CoroutineRunner.Instance.StartCoroutine(SendLLMQuery());
    }

    public override void Update()
    {
        if (currentlyThinking) return;
        Match match = Regex.Match(lastLLMResponse, coordinatePattern);

        if (match.Success)  // If the answer contains a grod coordinate of the form (x,y)
        {
            int x = int.Parse(match.Groups[1].Value);
            int y = int.Parse(match.Groups[2].Value);
            Debug.Log($"GridPosition: X= {x}, Y= {y}");
            if (x < worldInfo.WorldSize.x && y < worldInfo.WorldSize.y) // Set new Taget IF the new coordinates actually lie within the world Bounds
                // [NOTE] This setup doesn't account for walls. The NPC will likely walk into walls. A lot. 
                //        (Not that they didn't do that before anyway.)
                //        That is if the AI actually choses a coordinate different from (2,0) for once....
                //        A more intelligent AI with faster polling might even be capable of doing actuall Pathfinding, but my current ways of
                //        achieving that are either too slow (RAG is expensive, yo) or too stupid (gemma3:4b isn't that smart, but more useful
                //        than deepseek tbh).
                npc.TargetXZ = new Vector2(x + .5f, y + .5f) * worldInfo.WorldScaleFactor;
        }

        CoroutineRunner.Instance.StartCoroutine(SendLLMQuery());
    }

    IEnumerator SendLLMQuery()
    {
        string worldSizeString = $"{worldInfo.WorldSize.x} by {worldInfo.WorldSize.y}";
        string currentGridPosString = $"({npc.CurrentXZ.x},{npc.CurrentXZ.y})";
        string playerGridPosString = GetPlayerGridPosition().ToString();

        string userInput =
            $"<Context>You are an NPC on a {worldSizeString} sized grid." +
            $"Your current position is {currentGridPosString}" +
            $"and the Player is at Position {playerGridPosString}</Context>" +
            "<Task>Chose a position on the grid where you want to go.</Task>" +
            "<Contraints><Contraint>Your answer should only consist of the x,y coordinate of your target point.</Constraint>" +
            "<Constraint>Stay within the specified grid size</Contraint>" +
            $"<Constraint>(x,y) may not be larger than {worldSizeString}</Contraint></Constraints>" +
            "<Examples><Example>(0,0)</Example><Example>(1,2)</Example><Example>(3,3)</Example></Examples>";

        currentlyThinking = true;
        yield return OpenAIQuery.SendChatRequest(userInput, response =>
        {
            lastLLMResponse = response;
            Debug.Log("Antwort vom LLM: " + response);
            currentlyThinking = false;
        });
    }

    /// <summary>
    /// Takes the PlayerTransforms current position and returns the player's position as grid coordinates
    /// </summary>
    /// <returns>The player's grid coordinates</returns>
    (int, int) GetPlayerGridPosition()
    {
        Vector3 playerPos = playerTransform.position;
        Vector2 playerXZ = new Vector2(playerPos.x, playerPos.y);
        int gridX = Mathf.FloorToInt(playerXZ.x / worldInfo.WorldScaleFactor);
        int gridY = Mathf.FloorToInt(playerXZ.y / worldInfo.WorldScaleFactor);

        gridX = Mathf.Clamp(gridX, 0, Mathf.FloorToInt(worldInfo.WorldSize.x) - 1);
        gridY = Mathf.Clamp(gridY, 0, Mathf.FloorToInt(worldInfo.WorldSize.y) - 1);

        return (gridX, gridY);
    }
}

public interface IControllable
{
    Vector2 SpawnXZ { get; }
    Vector2 CurrentXZ { get; }
    Vector2 TargetXZ { get; set; }
    Transform Player { get; set; }
    public void SetRunning();
    public void SetWalking();
}