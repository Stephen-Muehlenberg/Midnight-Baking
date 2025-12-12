using System;
using System.Collections.Generic;
using Godot;

namespace MidnightBaking.scripts;

public static class Player
{
    public static Interactable heldItem { get; private set; }
    public static bool canHoldItem => heldItem == null;
    private static readonly List<Action> OnItemHoldChangedCallbacks = new();

    public static void RegisterItemHoldChangedListener(Action callback)
    {
        OnItemHoldChangedCallbacks.Add(callback);
    }

    public static void HoldItem(Interactable item)
    {
        if (heldItem != null)
            throw new Exception($"Cannot hold 2 items at once!");
        
        heldItem = item;
        item.Reparent(PlayerController.instance.heldObjectPoint);
        item.Position = Vector3.Zero;
        item.RotationDegrees = Vector3.Zero;
        foreach (var callback in OnItemHoldChangedCallbacks)
            callback.Invoke();
    }

    public static void StopHoldingItem()
    {
        heldItem = null;
        foreach (var callback in OnItemHoldChangedCallbacks)
            callback.Invoke();
    }
}