using Godot;
using Godot.Collections;
using LabyrinthExplorer3D.scripts.game.abilties;
using LabyrinthExplorer3D.scripts.game.npc;

namespace LabyrinthExplorer3D.scripts.game.characters.nodes;

[GlobalClass]
public partial class EarsArea3D : Area3D
{
    [Export] public bool TrackPlayers;
    [Export] public Array<Player3D> PlayersInRange;

    [Export] public bool TrackNpcs;
    [Export] public Array<Npc3D> NpcsInRange;
    
    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

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
    
    private void OnBodyEntered(Node3D body)
    {
        if (TrackPlayers && body is Player3D player)
        {
            PlayersInRange.Add(player);
            GD.Print($"[INFO] [{Name}] {player.PlayerID}/{player.Name} has entered ears area");
        }

        if (TrackNpcs && body is Npc3D npc)
        {
            NpcsInRange.Add(npc);
            GD.Print($"[INFO] [{Name}] {npc.Name} has entered ears area");       
        }
    }
    private void OnBodyExited(Node3D body)
    {
        if (TrackPlayers && body is Player3D player)
        {
            PlayersInRange.Remove(player);
            GD.Print($"[INFO] [{Name}] {player.PlayerID}/{player.Name} has entered ears area");
        }

        if (TrackNpcs && body is Npc3D npc)
        {
            NpcsInRange.Remove(npc);
            GD.Print($"[INFO] [{Name}] {npc.Name} has left ears area");       
        }
    }
}