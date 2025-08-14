using System;
using UnityEngine;
using UnityEngine.InputSystem;
public partial class NpcStateManager
{
    public void SetRunning()
    {
        this.IsRunning = true;
        this.IsCrouching = false;
        this.CurrentSpeed = this.RunSpeed;
    }

    public void SetWalking()
    {
        this.IsRunning = false;
        this.IsCrouching = false;
        this.CurrentSpeed = this.WalkSpeed;
    }
}
