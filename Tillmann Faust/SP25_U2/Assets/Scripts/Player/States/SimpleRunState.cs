using UnityEngine;

public class SimpleRunState : BaseState
{
    protected override string STATE_NAME { get; } = "simple running";
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
        if (!character.IsRunning)
            character.SwitchState(character.WalkingState);
        else if (character.MoveVector.magnitude == 0)
            character.SwitchState(character.RunIdleState);
        else
            character.Move();

    }
}
