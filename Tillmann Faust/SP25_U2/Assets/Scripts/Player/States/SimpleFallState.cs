using System;
using UnityEngine;

public class SimpleFallState : BaseState
{
    protected override string STATE_NAME { get; } = "simple falling";
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
        if (character.controller.isGrounded)
            character.SwitchState(character.IdlingState);
        else
            character.Move();
    }
}
