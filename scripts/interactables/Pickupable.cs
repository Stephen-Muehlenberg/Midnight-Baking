using System;
using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace MidnightBaking.scripts.interactables;

/// <summary>
/// An object the player can pick up and move around.
/// </summary>
public partial class Pickupable : Interactable
{
    [Export] public Node3D initialPosition { get; private set; }

    public void PickUp()
    {
        if (!Player.CanInteract(this))
            throw new Exception($"Attempted to pick up item {Name} but player is already holding {Player.HeldItemsToString()}.");
        Player.PickUpItem(this);
    }

    protected override void _ResetToGameStartState()
    {
        // Reset position and rotation to fridge starting state.
        SetPlacement(initialPosition);
    }

    public override void Save(Dictionary data)
    {
        data["position"] = GetParent().Name.ToString();
    }

    public override void Load(Dictionary data)
    {
        if (data == null || data.Count == 0) return;
        var parentVariant = data["position"];
        if (parentVariant.Obj == null || parentVariant.VariantType != Variant.Type.String)
            throw new Exception("Pickupable \"{Name}\"'s saved position data is null or the type is not a string.");
        if (initialPosition == null)
            throw new Exception($"Pickupable \"{Name}\"'s initial position is null. Make sure to set it in the inspector.");
        
        var parentName = parentVariant.AsString();
        if (initialPosition.Name.ToString() == parentName)
        {
            SetPlacement(initialPosition);
            return;
        }

        foreach (Node3D position in GetLocations())
        {
            if (position.Name.ToString() != parentName) continue;
            SetPlacement(position);
            return;
        }

        throw new Exception($"Tried to set \"{Name}\"'s parent to \"{parentName}\" but it was not found.");
    }

    // TODO this should probably be abstract instead of virtual.
    /// <summary>
    /// Pickupables should override this so that the base class can know about their possible positions.
    /// This allows the base class to save and load position, rather than requiring each subclass to implement
    /// save/load functionality.
    /// </summary>
    /// <returns></returns>
    protected virtual Node3D[] GetLocations() => [];
}