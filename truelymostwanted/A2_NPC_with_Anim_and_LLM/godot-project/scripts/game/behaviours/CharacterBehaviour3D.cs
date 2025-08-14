using Godot;
using LabyrinthExplorer3D.scripts.game.characters;

namespace LabyrinthExplorer3D.scripts.game.behaviours;

[GlobalClass]
public abstract partial class CharacterBehaviour3D : Node3D
{
    [Export] public Character3D OwningPlayer;
}