using Godot;
using Godot.Collections;
using LabyrinthExplorer3D.scripts.game.abilties;
using LabyrinthExplorer3D.scripts.game.characters;
using LabyrinthExplorer3D.scripts.game.components;

[GlobalClass]
public partial class Player3D : Character3D
{
	[Export] public int PlayerID;
	[Export] public string PlayerName;
	
	public override void _UnhandledInput(InputEvent @event)
	{
		foreach(var ability in Abilities)
			ability._OnUnhandledInput(@event);
	}
	public override void _Process(double delta)
	{
		foreach(var ability in Abilities)
			ability._OnProcess(delta);
	}
}
