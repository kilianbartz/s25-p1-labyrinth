using Godot;
using Godot.Collections;
using LabyrinthExplorer3D.scripts.game.abilties;
using LabyrinthExplorer3D.scripts.game.behaviours;
using LabyrinthExplorer3D.scripts.game.characters.nodes;
using LabyrinthExplorer3D.scripts.game.components;

namespace LabyrinthExplorer3D.scripts.game.characters;

[GlobalClass]
public partial class Character3D : CharacterBody3D
{    
    [Export] public CharacterModel3D CharacterModel;
    
    [Export] public Node3D RightHandParent;
    [Export] public Node3D LeftHandParent;
	
    [Export] public Array<Character3dAbility> Abilities;
    [Export] public Array<CharacterBehaviour3D> Behaviours;
    [Export] public Array<Component> Components;

    [Export] public Camera3D Eyes;
    [Export] public EarsArea3D Ears;

    public bool TryGetAbility<T>(out T ability) where T : Character3dAbility
        => Abilities.TryGetTypeOf(out ability);
    public bool TryGetBehaviour<T>(out T behaviour) where T : CharacterBehaviour3D
        => Behaviours.TryGetTypeOf(out behaviour);
    public bool TryGetComponent<T>(out T component) where T : Component
        => Components.TryGetTypeOf(out component);
}