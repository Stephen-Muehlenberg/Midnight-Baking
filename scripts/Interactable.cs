using System;
using System.Collections.Generic;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public partial class Interactable : Node3D
{
    [Export] public Game.ItemId itemId;
    [Export] public bool interactionRequiresFreeHands = false;
    /// <summary>Meshes to highlight when this object's interactions are enabled.</summary>
    [Export] public MeshInstance3D[] highlightMeshes;
    /// <summary>CSGs to highlight when this object's interactions are enabled.</summary>
    [Export] public CsgPrimitive3D[] highlightCsgs;
    
    protected bool interactionsOn = true;
    protected List<Action> onClickCallbacks = new();
    protected List<Action> highlightRequests = new();
    
    public enum Requirement { ANY, ONE_HAND, BOTH_HANDS }

    protected Dictionary<Action, Requirement> pendingInteractions = new();

    public void AddInteraction(Action callback, Requirement requirement = Requirement.ANY)
    {
        if (pendingInteractions.ContainsKey(callback))
        {
            // TODO Exception
            return;
        }
        pendingInteractions[callback] = requirement;
    }
    
    public override void _Ready()
    {
        Game.RegisterInteractable(this);
        if (interactionRequiresFreeHands)
            Player.RegisterItemHoldChangedListener(UpdateHighlights);
    }

    public void ResetToGameStartState() => _ResetToGameStartState();
    
    /// <summary>
    /// Optional hook method for subclasses to override. Invoked during <see cref="ResetToGameStartState()"/>.
    /// </summary>
    protected virtual void _ResetToGameStartState() { }

    /// <summary>
    /// Set whether the user can interact with this object.
    /// </summary>
    public void SetCanInteract(bool canInteract)
    {
        interactionsOn = canInteract;
        UpdateHighlights();
    }

    /// <summary>
    /// Register a callback to be invoked when this Interactable is clicked. By default, marks the interactable as
    /// "should be highlighted", though this can be skipped. Note that this Interactable may still be highlighted
    /// even if <paramref name="highlight"/> is set to false, if there are any other click listeners which set it to
    /// true.
    /// </summary>
    public void AddClickListener(Action callback, bool highlight = true, bool requiresEmptyHands = false)
    {
        if (onClickCallbacks.Contains(callback))
            throw new Exception($"Attempted to add a duplicate onClick callback to Interactable \"{Name}\". Callbacks count = {onClickCallbacks.Count}.");

        onClickCallbacks.Add(callback);
        
        if (highlight)
        {
            highlightRequests.Add(callback);
            if (highlightRequests.Count == 1)
                UpdateHighlights();
        }
    }

    public void RemoveClickListener(Action callback)
    {
        // Due to being able to optionally skip steps, it's important to be able to 
        // ATTEMPT to remove callbacks even if they aren't there. So calling
        // Remove() even when a callback hasn't been Added is a completely valid case.
        if (!onClickCallbacks.Contains(callback))
            return;
        
        onClickCallbacks.Remove(callback);
        highlightRequests.Remove(callback);
        if (highlightRequests.Count == 0)
            UpdateHighlights();
    }
    
    private void UpdateHighlights()
    {
        Material material = interactionsOn && highlightRequests.Count > 0 && (!interactionRequiresFreeHands || Player.canHoldItem)
            ? Game.flashMaterialOverlay
            : null;
        
        foreach (var mesh in highlightMeshes)
            mesh.MaterialOverlay = material;
        foreach (var csg in highlightCsgs)
            csg.MaterialOverlay = material;
    }

    public void PickUp()
    {
        if (!Player.canHoldItem)
            throw new Exception($"Attempted to pick up item {Name} but player is already holding item {Player.heldItem.Name}.");
        Player.HoldItem(this);
    }

    /// <summary>
    /// Sets this Interactable as a child of the specified parent, and resets its position and rotation to 0
    /// relative to the new parent.
    /// </summary>
    public void SetPlacement(Node3D newParent)
    {
        // TODO figure out how to handle this during scene setup. A node's parent is readied AFTER its children.
        if (GetParent() == null || newParent == null)
        {
            GD.Print($"{Name}.Interactable tried to call SetPlacement() but the {(newParent == null ? "new" : "old")} parent was null.");
            return;
        }

        if (Player.heldItem == this)
            Player.PlaceItem(newParent);
        else
        {
            // Using Reparent() during scene setup causes errors.
            // So we use CallDeferred() to indirectly invoke Reparent() on the next free frame.
            // This ensures Reparent() is only called once everything is set up.
            CallDeferred("reparent", newParent);
            Position = Vector3.Zero;
            RotationDegrees = Vector3.Zero;
        }
    }

    /// <summary>
    /// Should be invoked by this object's associated collider (often a child),
    /// via its attached <see cref="InteractionTarget"/>.
    /// </summary>
    public void OnInteractableClicked()
    {
        if (!interactionsOn) return;
        _HandleClick();
        // Iterate backwards in case the current item removes itself from the list.
        for (int i = onClickCallbacks.Count - 1; i >= 0; i--)
            onClickCallbacks[i].Invoke();
    }
    
    /// <summary>
    /// Optional hook method for subclasses to override. Invoked during <see cref="OnInteractableClicked()"/>.
    /// </summary>
    protected virtual void _HandleClick() {}
}
