using Godot;
using LabyrinthExplorer3D.scripts.game.abilties;
using LabyrinthExplorer3D.scripts.game.ai.data;

namespace LabyrinthExplorer3D.scripts.game.npc.ability;

[GlobalClass]
public partial class ResponseNpcAbility3d : Character3dAbility
{
    public void UseResponse(CharacterState charState)
    {
        var isIdle = charState == CharacterState.Idle;
        var isWalking = charState == CharacterState.Walk;
        var isSprinting = charState == CharacterState.Run;
        var isSneaking = charState == CharacterState.Sneak;
        var isWaving = charState == CharacterState.WaveL;
        
        var animationTree = OwningCharacter.CharacterModel.CharacterAnimationTree;
        animationTree.Set("parameters/conditions/IsStanding", isIdle);
        animationTree.Set("parameters/conditions/IsWalking", isWalking);
        animationTree.Set("parameters/conditions/IsRunning", isSprinting);
        animationTree.Set("parameters/conditions/IsSneaking", isSneaking);
        animationTree.Set("parameters/conditions/IsWaving", isWaving);
    }

    public void UseResponse(NpcReaction npcReaction)
    {
        UseResponse(npcReaction.Reaction);
    }
    
    public override void _OnProcess(double delta)
    {
    }

    public override void _OnUnhandledInput(InputEvent @event)
    {
    }
}