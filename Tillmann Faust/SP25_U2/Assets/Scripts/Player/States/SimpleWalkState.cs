using UnityEngine;

public class SimpleWalkState : BaseState
{
    protected override string STATE_NAME { get; } = "simple walking";
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
        if (character.MoveVector.magnitude == 0)        // If Walking and not moving
            character.SwitchState(character.IdlingState);
        //else if (character.IsCrouching)                 // If Walking and Crouching
        //    character.SwitchState(character.CrouchWalkState);
        else if (character.IsRunning)                   // If idle and Running
            character.SwitchState(character.RunState);
        else                                            // If just Walking
            character.Move();
    }
}
