using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public partial class PlayerStateManager : EntityStateManager
{
    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<PlayerInput>();
    }
    protected override void Start()
    {
        base.Start();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ApplyGravity();
        if (CurrentState != FallingState && !controller.isGrounded)
        {
            SwitchState(FallingState);
        }
        CurrentState.UpdateState(this);
    }

    #region Movement & Rotation
    private void LateUpdate()
    {
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, RotateVector.y, 0f);

        VerticalLookRotation = Mathf.Clamp(RotateVector.x, -90f, 90f);
        CameraHolder.localRotation = Quaternion.Euler(-VerticalLookRotation, 0f, 0f);
    }
    #endregion
}
