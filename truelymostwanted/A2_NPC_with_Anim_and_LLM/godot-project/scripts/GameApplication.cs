using System.Threading;
using System.Threading.Tasks;
using Godot;
using LabyrinthExplorer3D.scripts.core.functions;
using LabyrinthExplorer3D.scripts.game.ai;

namespace LabyrinthExplorer3D.scripts;

[GlobalClass]
public partial class GameApplication : Node
{
    public override void _Ready()
    {
        base._Ready();
        StopGameFunction.Execute(node: this);
    }
}