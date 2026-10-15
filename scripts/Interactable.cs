using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

/// <summary>
/// A physical object in the scene which the player can interact with.
/// <br/><br/>The object's name <b>must</b> uniquely match with a corresponding <see cref="Item.Id"/>.
/// Case, spaces, and underscores are ignored when checking for a match, e.g. "Foo 1", "foo1", and "FOO_1"
/// are interchangeable.
/// </summary>
public partial class Interactable : Node3D, Resetable
{
    public Item.Id idTest { get; private set; }
    [Export] public bool interactionRequiresFreeHands = false;
    /// <summary>Meshes to highlight when this object's interactions are enabled.</summary>
    [Export] public MeshInstance3D[] highlightMeshes;
    /// <summary>CSGs to highlight when this object's interactions are enabled.</summary>
    [Export] public CsgPrimitive3D[] highlightCsgs;
    
    protected bool interactionsOn = true;
    protected List<Action> onClickCallbacks = new();
    protected List<Action> highlightRequests = new();
    
    public enum Requirement { ANY, ONE_HAND, BOTH_HANDS }

    protected System.Collections.Generic.Dictionary<Action, Requirement> pendingInteractions = new();

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
        idTest = Item.RegisterInteractable(this);
        if (interactionRequiresFreeHands)
            Player.RegisterItemHoldChangedListener(UpdateHighlights);
    }

    /// <summary>
    /// Set this Interactable's state based on a game state stored as json.
    /// </summary>
    public virtual void LoadState(Dictionary json) { }

    /// <summary>
    /// Save this Interactable's state to json.
    /// </summary>
    public virtual void SaveState(Dictionary json) { }

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

        // Using Reparent() during scene setup (and possibly other times) causes errors.
        // So we use CallDeferred() to indirectly invoke Reparent() on the next free frame.
        // This ensures Reparent() is only called once everything is set up.
        CallDeferred("ReparentDeferred", newParent);
    }

    /// <summary>
    /// Companion method to <see cref="SetPlacement()"/>. Should not be invoked by any other method.
    /// </summary>
    private void ReparentDeferred(Node3D newParent)
    {
        Reparent(newParent);
        Position = Vector3.Zero;
        RotationDegrees = Vector3.Zero;
        if (Player.heldItem == this)
            Player.StopHoldingItem();
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
