using Godot;

namespace LabyrinthExplorer3D.scripts.game.characters;

[GlobalClass]
public partial class CharacterModel3D : Node3D
{
    [Export] public Skeleton3D CharacterRig;
    [Export] public MeshInstance3D CharacterSkeleton;
    [Export] public AnimationPlayer CharacterAnimationPlayer;
    [Export] public AnimationTree CharacterAnimationTree;
}