using Godot;
using LabyrinthExplorer3D.scripts.game.ai.data;

namespace LabyrinthExplorer3D.scripts.game.components;

[GlobalClass]
public partial class CharacterStateComponent : Component
{
    [Export] public CharacterState CharacterState;
}