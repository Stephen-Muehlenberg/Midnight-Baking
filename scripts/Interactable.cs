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
        DisableInteractions();
        _ResetToGameStartState();
    }
    protected virtual void _ResetToGameStartState() { }

    public void EnableInteractions(Action onClick) => SetInteractionsOn(true, onClick);
    public void DisableInteractions() => SetInteractionsOn(false, null);
    
    private void SetInteractionsOn(bool on, Action onClick)
    {
        GD.Print($"{Name}.SetInteractionsOn({on})");
        interactionsOn = on;
        onClickCallback = onClick;
        foreach (var mesh in highlightMeshes)
            mesh.MaterialOverlay = on ? Game.flashMaterialOverlay : null;
        foreach (var csg in highlightCsgs)
            csg.MaterialOverlay = on ? Game.flashMaterialOverlay : null;
    }

    /// <summary>
    /// Should be invoked by this object's associated collider (often a child),
    /// via its attached <see cref="InteractionTarget"/>.
    /// </summary>
    public void OnClick()
    {
        onClickCallback?.Invoke();
    }
}
