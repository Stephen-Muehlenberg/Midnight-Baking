using System;
using Godot;

namespace MidnightBaking.scripts;

public partial class Interactable : Node3D
{
    [Export] public Game.ItemId itemId;
    /// <summary>Meshes to highlight when this object's interactions are enabled.</summary>
    [Export] public MeshInstance3D[] highlightMeshes;
    /// <summary>CSGs to highlight when this object's interactions are enabled.</summary>
    [Export] public CsgPrimitive3D[] highlightCsgs;
    
    protected bool interactionsOn;
    protected Action onClickCallback;

    public override void _Ready()
    {
        Game.RegisterInteractable(this);
    }

    public void ResetToGameStartState()
    {
        _ResetToGameStartState();
    }
    protected virtual void _ResetToGameStartState() { }

    /// <summary>
    /// Set whether the user can interact with this object.
    /// </summary>
    public void SetCanInteract(bool canInteract) => SetInteractions(canInteract, null);

    /// <summary>
    /// Enable interactions and highlight this object.
    /// </summary>
    public void SetHighlighted(Action onClickCallback) => SetInteractions(true, onClickCallback);
    
    private void SetInteractions(bool on, Action onClick)
    {
        interactionsOn = on;
        onClickCallback = onClick;
        
        bool showHighlight = onClick != null; // Objects are only highlighted if there is a callback.
        foreach (var mesh in highlightMeshes)
            mesh.MaterialOverlay = showHighlight ? Game.flashMaterialOverlay : null;
        foreach (var csg in highlightCsgs)
            csg.MaterialOverlay = showHighlight ? Game.flashMaterialOverlay : null;
    }

    /// <summary>
    /// Should be invoked by this object's associated collider (often a child),
    /// via its attached <see cref="InteractionTarget"/>.
    /// </summary>
    public void OnInteractableClicked()
    {
        if (!interactionsOn) return;
        HandleClick();
        onClickCallback?.Invoke();
    }
    protected virtual void HandleClick() {}
}
