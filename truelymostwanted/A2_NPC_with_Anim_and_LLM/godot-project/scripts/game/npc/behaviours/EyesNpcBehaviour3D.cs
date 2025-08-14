using Godot;
using Godot.Collections;
using LabyrinthExplorer3D.scripts.game.behaviours;
using LabyrinthExplorer3D.scripts.game.player;

namespace LabyrinthExplorer3D.scripts.game.npc.behaviours;

[GlobalClass]
public partial class EyesNpcBehaviour3D : CharacterBehaviour3D
{
    [Export] public bool SeesPlayer;
    [Export] public float DistanceToPlayer;
    [Export] public double TimeInSight;
    [Export] public double TimeSinceLastSeen;
    
    
    [Export] public Sprite3D IsViewingSprite3D;
    [Export] public Sprite3D IsNotViewingSprite3D;
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var globalPos = PlayerController3D.Instance.CurrentPlayer.GlobalPosition;
        SeesPlayer = OwningPlayer.Eyes.IsPositionInFrustum(globalPos);
        IsViewingSprite3D.Visible = SeesPlayer;
        IsNotViewingSprite3D.Visible = !SeesPlayer;
        if (SeesPlayer)
        {
            DistanceToPlayer = OwningPlayer.GlobalPosition.DistanceTo(globalPos);
            TimeInSight += delta;
            TimeSinceLastSeen = 0;       
        }
        else
        {
            DistanceToPlayer = -1;
            TimeInSight = 0;       
            TimeSinceLastSeen += delta;       
        }
    }
}