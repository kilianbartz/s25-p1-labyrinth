using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SimpleCrouchWalkState : BaseState
{
    protected override string STATE_NAME { get; } = "simple crouch walk";

    public override void EnterState(EntityStateManager character)
    {
        base.EnterState(character);
        character.CurrentSpeed = character.CrouchSpeed;
    }

    public override void ExitState(EntityStateManager character)
    {
        base.ExitState(character);
    }
    public override void UpdateState(EntityStateManager character)
    {
        if (!character.IsCrouching)                         // If crouch-walking and not crouching
            character.SwitchState(character.WalkingState);
        //else if (character.MoveVector.magnitude == 0)       // If crouch-walking and not moving
        //    character.SwitchState(character.CrouchIdleState);
        else                                                // If just crouch-walking
            character.Move();
    }
}
