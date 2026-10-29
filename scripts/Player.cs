using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public static class Player
{
    public static Interactable heldItemLeft { get; private set; }
    public static Interactable heldItemRight { get; private set; }
    private static readonly List<Action> OnItemHoldChangedCallbacks = new();

    public static void RegisterItemHoldChangedListener(Action callback)
        => OnItemHoldChangedCallbacks.Add(callback);
    
    public static void RemoveItemHoldChangedListener(Action callback)
        => OnItemHoldChangedCallbacks.Remove(callback);

    public static string HeldItemsToString()
    {
        if (heldItemLeft == heldItemRight) return $"{heldItemLeft?.Name?.ToString() ?? "nothing"} (both hands)";
        return $"{heldItemLeft?.Name?.ToString() ?? "nothing"} (left hand) and {heldItemRight?.Name?.ToString() ?? "nothing"} (right hand)";
    }

    /// <summary>
    /// Returns true if the player is holding the specified item (in one or both hands).
    /// If no item is specified (null argument), then returns true only if <b>both</b>
    /// hands are empty.
    /// </summary>
    public static bool IsHolding(Interactable item)
    {
        if (item == null) return heldItemLeft == null && heldItemRight == null;
        return heldItemLeft == item || heldItemRight == item;
    }
    
    public static bool CanInteract(Interactable item) => CanInteract(item.requirement);
    public static bool CanInteract(Interactable.Requirement interactionRequirement)
    {
        if (interactionRequirement == Interactable.Requirement.NO_REQUIREMENT) return true;
        if (interactionRequirement == Interactable.Requirement.REQUIRES_ONE_HAND &&
            (heldItemLeft == null || heldItemRight == null)) return true;
        return heldItemLeft == null && heldItemRight == null;
    }

    public static void HoldItem(Pickupable item)
    {
        if (!CanInteract(item))
            throw new Exception($"Cannot hold item \"{item.Name}\"; already holding \"{heldItemLeft.Name}\" and/or \"{heldItemRight.Name}\". Make sure to check if item can be held before attempting to hold it.");

        Debug.Print($"Player picked up {item.Name}");
        
        Node3D itemNewParent;
        if (item.requirement == Interactable.Requirement.REQUIRES_BOTH_HANDS)
        {
            heldItemLeft = item;
            heldItemRight = item;
            itemNewParent = PlayerController.instance.heldObjectPointCentre;
        }
        else if (heldItemRight == null)
        {
            heldItemRight = item;
            itemNewParent = PlayerController.instance.heldObjectPointRightHand;
        }
        else
        {
            heldItemLeft = item;
            itemNewParent = PlayerController.instance.heldObjectPointLeftHand;
        }
        
        // Reposition item
        item.Reparent(itemNewParent);
        item.Position = Vector3.Zero; // Relative to parent position.
        item.RotationDegrees = Vector3.Zero; // Relative to parent rotation.
        
        foreach (var callback in OnItemHoldChangedCallbacks)
            callback.Invoke();
    }

    public static void StopHoldingItem(Interactable item)
    {
        Debug.Print($"Player put down {item.Name}");
        
        if (heldItemLeft == item) 
            heldItemLeft = null;
        if (heldItemRight == item)
            heldItemRight = null;
        foreach (var callback in OnItemHoldChangedCallbacks)
            callback.Invoke();
    }
}