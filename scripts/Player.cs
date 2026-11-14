using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

/// <summary>
/// Public static methods for interacting with the player.
/// <br/>NOT the main player game object controller; see <see cref="PlayerController"/>.
/// </summary>
public static class Player
{
    private const float PICKUP_TWEEN_DURATION = 0.2f;
    
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

    /// <summary>
    /// Returns true if the player is holding the specified item in the left hand.
    /// If no item is specified (null argument), returns true if left hand is empty.
    /// </summary>
    public static bool IsHoldingInLeftHand(Interactable item)
    {
        if (item == null && heldItemLeft == null) return true;
        return heldItemLeft == item;
    }

    /// <summary>
    /// Returns true if the player is holding the specified item in the right hand.
    /// If no item is specified (null argument), returns true if right hand is empty.
    /// </summary>
    public static bool IsHoldingInRightHand(Interactable item)
    {
        if (item == null && heldItemRight == null) return true;
        return heldItemRight == item;
    }
    
    public static bool CanInteract(Interactable item) => CanInteract(item.requirement);
    public static bool CanInteract(Interactable.Requirement interactionRequirement)
    {
        if (interactionRequirement == Interactable.Requirement.NO_REQUIREMENT) return true;
        if (interactionRequirement == Interactable.Requirement.REQUIRES_ONE_HAND &&
            (heldItemLeft == null || heldItemRight == null)) return true;
        return heldItemLeft == null && heldItemRight == null;
    }

    public static void PickUpItem(Pickupable item, bool skipPickupAnimation = false, Action onAnimationEndCallback = null)
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

        item.Reparent(itemNewParent); // TODO This MIGHT cause errors; perhaps only if called during startup? See Interactable.SetPlacement().
        
        // Snap to new position...
        if (skipPickupAnimation)
        {
            item.Position = Vector3.Zero;
            item.RotationDegrees = Vector3.Zero;
            onAnimationEndCallback?.Invoke();
        }
        
        // ...Or animate to new position.
        else
        {
            Tween tween = Game.instance.GetTree().CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(item, "position", Vector3.Zero, PICKUP_TWEEN_DURATION);
            tween.TweenProperty(item, "rotation_degrees", Vector3.Zero, PICKUP_TWEEN_DURATION);
            if (onAnimationEndCallback != null)
            {
                tween.SetParallel(false);
                tween.TweenCallback(Callable.From(onAnimationEndCallback));
            }
        }
        
        foreach (var callback in OnItemHoldChangedCallbacks)
            callback.Invoke();
    }

    public static void PutDownItem(Pickupable item, Node3D newParent, bool skipAnimation = false, Action onAnimationEndCallback = null)
    {
        if (!IsHolding(item))
            throw new Exception($"Cannot put down item {item.Name}: player is not currently holding it. Currently holding: {heldItemLeft?.Name ?? "null"} (left) and {heldItemRight?.Name ?? "null"} (right).");
        Debug.Print($"Player put down {item.Name}");
        
        if (heldItemLeft == item) 
            heldItemLeft = null;
        if (heldItemRight == item)
            heldItemRight = null;
        
        item.Reparent(newParent); // TODO This MIGHT cause errors; perhaps only if called during startup? See Interactable.SetPlacement().
        
        // Snap to new position...
        if (skipAnimation)
        {
            item.Position = Vector3.Zero;
            item.RotationDegrees = Vector3.Zero;
            onAnimationEndCallback?.Invoke();
        }
        
        // ...Or animate to new position.
        else
        {
            Tween tween = Game.instance.GetTree().CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(item, "position", Vector3.Zero, PICKUP_TWEEN_DURATION);
            tween.TweenProperty(item, "rotation_degrees", Vector3.Zero, PICKUP_TWEEN_DURATION);
            if (onAnimationEndCallback != null)
            {
                tween.SetParallel(false);
                tween.TweenCallback(Callable.From(onAnimationEndCallback));
            }
        }

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

    public static void EnableNormalControls(bool enable)
    {
        PlayerController.instance.SetEnabled(enable);
    }

    public static void MoveTo(Vector3 position, Vector2 rotation, bool animate = true, Action onAnimationEndCallback = null)
    {
        Quaternion bodyRotation = Quaternion.FromEuler(new Vector3(0, Mathf.DegToRad(rotation.Y), 0));
        Quaternion headRotation = Quaternion.FromEuler(new Vector3(Mathf.DegToRad(rotation.X), 0, 0));
        if (animate)
        {
            Tween tween = Game.instance.GetTree().CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(PlayerController.instance, "position", position, 0.5f);
            tween.TweenProperty(PlayerController.instance, "quaternion", bodyRotation, 0.5f);
            tween.TweenProperty(PlayerController.instance.head, "quaternion", headRotation, 0.5f);
            if (onAnimationEndCallback != null)
            {
                tween.SetParallel(false);
                tween.TweenCallback(Callable.From(onAnimationEndCallback));
            }
        }
        else
        {
            PlayerController.instance.Position = position;
            PlayerController.instance.Quaternion = bodyRotation;
            PlayerController.instance.head.Quaternion = headRotation;
        }
    }

    public static (Vector3, Vector2) GetPositionAndRotation()
    {
        return (PlayerController.instance.Position, 
            new Vector2(
                PlayerController.instance.head.RotationDegrees.X,
                PlayerController.instance.RotationDegrees.Y));
    }

    public static void Debug_LogHeldItems()
    {
        Debug.Print($"Player is holding {(heldItemLeft == null ? "nothing" : heldItemLeft.Name.ToString())} (left) and {(heldItemRight == null ? "nothing" : heldItemRight.Name.ToString())} (right).");
    }
}