using UnityEngine;

public class SimpleRunIdleState : BaseState
{
    protected override string STATE_NAME { get; } = "simple run idle";

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
        if (!character.IsRunning)                   // If Running Idle and not running
            character.SwitchState(character.IdlingState);
        else if (character.MoveVector.magnitude != 0)    // If Running Idle and moving
            character.SwitchState(character.RunState);
    }
}
