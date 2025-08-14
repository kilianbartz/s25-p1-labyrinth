namespace LabyrinthExplorer3D.scripts.game.ai.data;

public enum Personality
{
    Neutral = 0,
    Cautious = 1,
    Aggressive = 2,
}
public enum CharacterState
{
    Unknown = -1,
    Idle = 0,
    WaveL = 1,
    Walk = 2,
    Run = 3,
    Sneak = 4
}

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

