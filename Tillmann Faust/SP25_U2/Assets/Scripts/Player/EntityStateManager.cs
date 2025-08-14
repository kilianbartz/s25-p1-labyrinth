using UnityEngine;

public abstract partial class EntityStateManager : MonoBehaviour
{
    protected virtual void Awake()
    {
        controller = GetComponent<CharacterController>();
        CurrentSpeed = WalkSpeed;
        IsCrouching = false;

        _gravityVector = new Vector3(0, -9.81f, 0);
    }
    protected virtual void Start()
    {
        CurrentState = IdlingState;
        CurrentState.EnterState(this);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
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

    private void LateUpdate()
    {
        ApplyRotation();
    }

    public void SwitchState(BaseState state)
    {
        CurrentState.ExitState(this);
        CurrentState = state;
        CurrentState.EnterState(this);
    }

    #region Movement & Rotation
    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, RotateVector.y, 0f);

        VerticalLookRotation = Mathf.Clamp(RotateVector.x, -90f, 90f);
        RotationReference.localRotation = Quaternion.Euler(-VerticalLookRotation, 0f, 0f);
    }

    public void ApplyGravity()
    {
        controller.Move(_gravityVector * Time.deltaTime);
    }

    public virtual void Move()
    {
        Vector3 move = transform.forward * MoveVector.z + transform.right * MoveVector.x;
        //Debug.Log(move);
        controller.Move(move * CurrentSpeed * Time.deltaTime);
    }
    #endregion
}
