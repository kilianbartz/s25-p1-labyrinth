using Godot;
using LabyrinthExplorer3D.scripts.game.components;

namespace LabyrinthExplorer3D.scripts.game.abilties;

[GlobalClass]
public partial class Character3dInventoryNextSlotAbility : Character3dAbility
{
    public override void _OnProcess(double delta)
    {
        
    }

    public override void _OnUnhandledInput(InputEvent @event)
    {
        if (!IsAnyInputActionTriggered())
            return;

        var canGet = OwningCharacter.TryGetComponent<InventoryComponent>(out var inventoryComponent);
        if (!canGet)
            return;
        
        inventoryComponent.Inventory.SelectNextSlot();
    }
}