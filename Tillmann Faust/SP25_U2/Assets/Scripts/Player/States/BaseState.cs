using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class BaseState
{
    protected abstract string STATE_NAME { get; }
    public virtual void EnterState(EntityStateManager character)
    {
        //Debug.Log($"Entering {STATE_NAME}");
        triggerCharacterAnimation(character);
    }
    public virtual void ExitState(EntityStateManager character)
    {
        //Debug.Log($"Exiting {STATE_NAME}");
    }
    public abstract void UpdateState(EntityStateManager character);

    protected void triggerCharacterAnimation(EntityStateManager character)
    {
        character.animator.SetTrigger(STATE_NAME);
    }
}
