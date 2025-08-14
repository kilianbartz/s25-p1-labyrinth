using UnityEngine;
using UnityEngine.InputSystem;
public partial class PlayerStateManager
{
    private void OnMovement(InputValue value)
    {
        InputVector = value.Get<Vector2>();
        MoveVector.x = InputVector.x;
        MoveVector.z = InputVector.y;

        //Debug.Log($"X move: {MoveVector.x}");
        //Debug.Log($"Z move: {MoveVector.z}");
    }

    private void OnCamera(InputValue value)
    {
        Vector2 InputVector = value.Get<Vector2>();
        RotateVector.x += InputVector.y;
        RotateVector.y = (RotateVector.y + InputVector.x) % 360;
        RotateVector.x = Mathf.Clamp(RotateVector.x, -90f, 90f);

        //Debug.Log($"X rotate: {RotateVector.x}");
        //Debug.Log($"Y rotate: {RotateVector.y}");
    }

#pragma warning disable CS0162 // Unreachable code detected
    private void OnCrouch(InputValue value)
    {
        return;
        IsRunning = false;
        IsCrouching = !IsCrouching;
        if (IsCrouching) CurrentSpeed = CrouchSpeed;
        else CurrentSpeed = WalkSpeed;

        Debug.Log($"Crouching: {IsCrouching}");
    }
#pragma warning restore CS0162 // Unreachable code detected

    private void OnRun(InputValue value)
    {
        IsCrouching = false;
        IsRunning = !IsRunning;
        if (IsRunning) CurrentSpeed = RunSpeed;
        else CurrentSpeed = WalkSpeed;

        Debug.Log($"Running: {IsRunning}");
    }
}
