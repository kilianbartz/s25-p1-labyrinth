using Godot;
using LabyrinthExplorer3D.scripts.core.functions;

namespace LabyrinthExplorer3D.scripts.core.inputactions;

[GlobalClass]
public partial class PauseGameInputActionAbility : InputActionAbility
{
    public override void _OnProcess(double delta)
    {
    }

    public override void _OnUnhandledInput(InputEvent @event)
    {
        if (!IsAnyInputActionTriggered())
            return;

        if (GetTree().IsPaused())
            ContinueGameFunction.Execute(node: this);
        else
            PauseGameFunction.Execute(node: this);
    }
}