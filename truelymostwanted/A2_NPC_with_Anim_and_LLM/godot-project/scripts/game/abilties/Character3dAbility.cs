using Godot;
using LabyrinthExplorer3D.scripts.game.characters;

namespace LabyrinthExplorer3D.scripts.game.abilties;

[GlobalClass]
public abstract partial class Character3dAbility : InputActionAbility
{
    public Character3D OwningCharacter
    {
        get
        {
            if(AbilityOwner is Character3D char3D)
                return char3D;
            return null;
        }
    }
}