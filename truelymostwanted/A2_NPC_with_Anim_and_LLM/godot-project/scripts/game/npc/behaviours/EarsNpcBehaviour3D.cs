using Godot;
using LabyrinthExplorer3D.scripts.game.behaviours;
using LabyrinthExplorer3D.scripts.game.player;

namespace LabyrinthExplorer3D.scripts.game.npc.behaviours;

[GlobalClass]
public partial class EarsNpcBehaviour3D : CharacterBehaviour3D
{
    [Export] public bool HearsPlayer;
    [Export] public float DistanceToPlayer;
    [Export] public double TimeWithAudio;
    [Export] public double TimeSinceLastAudio;

    [Export] public Sprite3D IsHearingSprite3D;
    [Export] public Sprite3D IsNotHearingSprite3D;
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        HearsPlayer = OwningPlayer.Ears.IsHearingAnyPlayer();
        IsHearingSprite3D.Visible = HearsPlayer;
        IsNotHearingSprite3D.Visible = !HearsPlayer;
        
        if (HearsPlayer)
        {
            DistanceToPlayer = OwningPlayer.GlobalPosition.DistanceTo(OwningPlayer.Ears.PlayersInRange[0].GlobalPosition);
            OwningPlayer.LookAt(OwningPlayer.Ears.PlayersInRange[0].GlobalPosition with { Y = OwningPlayer.GlobalPosition.Y });
            TimeWithAudio += delta;
            TimeSinceLastAudio = 0;       
        }
        else
        {
            TimeWithAudio = 0;       
            TimeSinceLastAudio += delta;       
        }
    }
}