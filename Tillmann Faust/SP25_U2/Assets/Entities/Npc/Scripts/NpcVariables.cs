using UnityEngine;
using UnityEngine.Rendering;

public partial class NpcStateManager : IControllable
{
    // AI Behavior ------------------------------------------------------------
    #region AI Behavior
    public Vector2 SpawnXZ { get; private set; }
    public Vector2 CurrentXZ { get; private set; }
    public Vector2 TargetXZ { get; set; }
    public Transform Player { get; set; }
    public BaseBehaviorController BehaviorController;
    public float timeElapsed;
    public const float behaviorChangeTime = 10f;
    public float PathfindingTolerance = 0.1f;
    public const float rotationSpeedPerFrame = 10f;
    // float DebugStep, DebugRemainingDistance, DebugDeltaTime, actualSPS;
    public Vector2 DebugUnitCoord, DebugTargetXZ;
    #endregion
}
