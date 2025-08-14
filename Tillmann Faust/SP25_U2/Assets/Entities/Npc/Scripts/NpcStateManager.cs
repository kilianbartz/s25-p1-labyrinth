//using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public partial class NpcStateManager : EntityStateManager
{
    protected override void Awake()
    {
        base.Awake();
        timeElapsed = 0;
        SpawnXZ = new Vector2(controller.transform.position.x, controller.transform.position.z);
        //BehaviorController = new PrimitiveBehaviorController(this, null, null, 2f, 5f);
    }
    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        BehaviorController.Update();
        DebugTargetXZ = TargetXZ;

        ApplyGravity();
        if (CurrentState != FallingState && !controller.isGrounded)
        {
            SwitchState(FallingState);
        }
        CurrentState.UpdateState(this);
        CalculateRotateAndMove();
    }

    #region Movement & Rotation
    private void LateUpdate()
    {
        //ApplyRotation();
    }

    private void ApplyRotation()
    {

        transform.rotation = Quaternion.Euler(0f, RotateVector.y, 0f);
    }

    public override void Move()
    {
        controller.Move(MoveVector);
    }

    public void CalculateRotateAndMove()
    {
        CurrentXZ = new Vector2(controller.transform.position.x, controller.transform.position.z);
        Vector2 ToTargetXZ = TargetXZ - CurrentXZ;
        float remainingDistance = ToTargetXZ.magnitude;

        // Stoppen, wenn Ziel erreicht
        if (remainingDistance <= PathfindingTolerance)
        {
            MoveVector = new Vector3(0f, 0f, 0f);
            return;
        }
        Vector3 ToTargetDirectionVector = new Vector3(ToTargetXZ.x, 0f, ToTargetXZ.y).normalized;

        // Zielrotation berechnen
        Quaternion targetRotation = Quaternion.LookRotation(ToTargetDirectionVector);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            RotateSpeed * Time.deltaTime
        );

        // Bewegung entlang Blickrichtung (nicht über Ziel hinaus)
        float step = Mathf.Min(CurrentSpeed * Time.deltaTime, remainingDistance);
        MoveVector = transform.forward * step;
    }
    #endregion
}
