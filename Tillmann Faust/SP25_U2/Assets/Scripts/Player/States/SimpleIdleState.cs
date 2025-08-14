using UnityEngine;

public class SimpleIdleState : BaseState
{
    protected override string STATE_NAME { get; } = "simple idle";

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
        //if (character.IsCrouching)                      // If Idle and Crouching
        //    character.SwitchState(character.CrouchIdleState);
        if (character.IsRunning)                   // If idle and Running
            character.SwitchState(character.RunIdleState);
        else if (character.MoveVector.magnitude != 0)   // If Idle and Moving
            character.SwitchState(character.WalkingState);
    }
}
