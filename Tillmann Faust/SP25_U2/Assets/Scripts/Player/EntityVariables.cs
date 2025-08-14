using UnityEngine;
using UnityEngine.Rendering;

public abstract partial class EntityStateManager
{
    // Concrete  --------------------------------------------------------------
    #region Concrete States
    public SimpleWalkState WalkingState = new SimpleWalkState();
    public SimpleIdleState IdlingState = new SimpleIdleState();
    public SimpleFallState FallingState = new SimpleFallState();
    //public SimpleCrouchIdleState CrouchIdleState = new SimpleCrouchIdleState();
    //public SimpleCrouchWalkState CrouchWalkState = new SimpleCrouchWalkState();
    public SimpleRunState RunState = new SimpleRunState();
    public SimpleRunIdleState RunIdleState = new SimpleRunIdleState();
    #endregion
    // Animation --------------------------------------------------------------
    #region Animation
    public Animator animator;
    #endregion
    // Speed Values -----------------------------------------------------------
    #region Entity Speeds
    public float CurrentSpeed;
    public float CrouchSpeed = 2.5f;
    public float WalkSpeed = 2.5f;
    public float RunSpeed = 5f;
    public float RotateSpeed = 180f;
    #endregion
    // Movement and Rotation --------------------------------------------------
    #region Move & Rotate
    public Vector3 MoveVector;
    public Vector2 RotateVector;
    public Transform RotationReference;
    public bool IsCrouching;
    public bool IsRunning;
    public float VerticalLookRotation;
    #endregion
    // Rest -------------------------------------------------------------------
    public CharacterController controller;
    public Vector3 controllerDefaultVec = new Vector3(0f, 1.1f, 0f);
    public Vector3 controllerCrouchIdleVec = new Vector3(0f, 1.5f, 0f);
    public Vector3 controllerCrouchWalkVec = new Vector3(0f, 1.5f, 0f);
    public BaseState CurrentState;
    public Vector3 _gravityVector;
}
