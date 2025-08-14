using UnityEngine;

public class SimpleCrouchIdleState : BaseState
{
    protected override string STATE_NAME { get; } = "simple crouch-idle";
    public override void EnterState(EntityStateManager character)
    {
        base.EnterState(character);
    }

    public override void ExitState(EntityStateManager character)
    {
        base.ExitState(character);
    }

    public override void UpdateState(EntityStateManager character)
    {
        if (!character.IsCrouching)                     // If Crouching and not crouching
            character.SwitchState(character.IdlingState);
        //if (character.MoveVector.magnitude != 0)        // If Crouching and moving
        //    character.SwitchState(character.CrouchWalkState);
    }
}
