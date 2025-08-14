using Godot;

namespace LabyrinthExplorer3D.scripts.game.npc;

[GlobalClass]
public partial class NpcController3D : Node3D
{
    public static NpcController3D Instance { get; private set; }

    [Export] public Npc3D CurrentNpc;
    
    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }
}