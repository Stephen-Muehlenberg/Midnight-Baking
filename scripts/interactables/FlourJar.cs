using System;
using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class FlourJar : Interactable
{
    [Export] private Node3D jarPlacement;
    
    protected override void _ResetToGameStartState()
    {
        // Reset position and rotation to fridge starting state.
        SetPlacement(jarPlacement);
    }
}