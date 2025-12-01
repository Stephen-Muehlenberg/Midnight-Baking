using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Pickupable : Interactable
{
    [Export] private Node3D initialPosition;
    
    protected override void _ResetToGameStartState()
    {
        // Reset position and rotation to fridge starting state.
        SetPlacement(initialPosition);
    }
}