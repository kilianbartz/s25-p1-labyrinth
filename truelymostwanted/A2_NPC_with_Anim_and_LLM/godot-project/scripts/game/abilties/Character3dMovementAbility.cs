using Godot;
using LabyrinthExplorer3D.scripts.game.ai.data;
using LabyrinthExplorer3D.scripts.game.components;

namespace LabyrinthExplorer3D.scripts.game.abilties;

[GlobalClass]
public partial class Character3dMovementAbility : Character3dAbility
{
    [Export] public float DefaultSpeed = 5.0f;
    [Export] public float SneakSpeed = 2.0f;
    [Export] public float SprintSpeed = 8.0f;

    [Export] public Vector2 InputVector2;
    [Export] public bool IsSprinting = false;
    [Export] public bool IsSneaking = false;

    public float GetSpeed()
    {
        if (IsSprinting && IsSneaking)
            return DefaultSpeed;
        if (IsSprinting)
            return SprintSpeed;
        if (IsSneaking)
            return SneakSpeed;
        return DefaultSpeed;   
    }

    public bool IsHearable()
    {
        return InputVector2 != Vector2.Zero && !IsSneaking;
    }
    
    public override void _OnProcess(double delta)
    {
        Vector3 velocity = OwningCharacter.Velocity;

        // Add the gravity.
        if (!OwningCharacter.IsOnFloor())
        {
            velocity += OwningCharacter.GetGravity() * (float)delta;
        }

        Vector2 inputDir = InputVector2;
        Vector3 direction = (OwningCharacter.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        float speed = GetSpeed();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * speed;
            velocity.Z = direction.Z * speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(OwningCharacter.Velocity.X, 0, speed);
            velocity.Z = Mathf.MoveToward(OwningCharacter.Velocity.Z, 0, speed);
        }

        OwningCharacter.Velocity = velocity;
        OwningCharacter.MoveAndSlide();
    }
    public override void _OnUnhandledInput(InputEvent @event)
    {
        if (!IsAnyInputActionTriggered())
            return;
        
        InputVector2 = Input.GetVector(
            InputActionNames[0], 
            InputActionNames[1], 
            InputActionNames[2], 
            InputActionNames[3]
        );
        IsSprinting = Input.IsActionPressed(InputActionNames[4]); // "player_sprint"
        IsSneaking = Input.IsActionPressed(InputActionNames[5]); // "player_sneak"

        if(!OwningCharacter.TryGetComponent<CharacterStateComponent>(out var component))
            return;
        SetCharacterState(component);
    }

    private void SetCharacterState(CharacterStateComponent component)
    {
        var isIdle = InputVector2 == Vector2.Zero;
        var isWalking = InputVector2 != Vector2.Zero;
        var isSprinting = IsSprinting;
        var isSneaking = IsSneaking;

        if (isIdle)
            component.CharacterState = CharacterState.Idle;
        if (isWalking)
            component.CharacterState = CharacterState.Walk;
        if (isSprinting)
            component.CharacterState = CharacterState.Run;
        if (isSneaking)
            component.CharacterState = CharacterState.Sneak;
        
        var animationTree = OwningCharacter.CharacterModel.CharacterAnimationTree;
        animationTree.Set("parameters/conditions/IsStanding", isIdle);
        animationTree.Set("parameters/conditions/IsWalking", isWalking);
        animationTree.Set("parameters/conditions/IsRunning", isSprinting);
        animationTree.Set("parameters/conditions/IsSneaking", isSneaking);
    }

    public Character3dMovementAbility()
    {
        InputActionNames =
        [
            "player_move_left",
            "player_move_right",
            "player_move_forward",
            "player_move_backward",
            "player_sprint",
            "player_sneak"
        ];
    }
    
}