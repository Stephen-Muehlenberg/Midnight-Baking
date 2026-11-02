using System.Diagnostics;
using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Door : Interactable
{
    protected const float OPEN_TWEEN_DURATION = 0.5f;
    
    [Export] protected Node3D doorPivot;
    [Export] protected float doorClosedY = 0;
    [Export] protected float doorOpenY = 90;
    
    public bool isOpen {get; protected set;}
    
    protected override void _ResetToGameStartState()
    {
        SetOpen(false);
        SetCanInteract(true);
    }
    
    /// <summary>
    /// Snap the door to the specified open/closed position.
    /// </summary>
    public virtual void SetOpen(bool open)
    {
        isOpen = open;
        var targetRotation = Quaternion.FromEuler(new Vector3(0, Mathf.DegToRad(open ? doorOpenY : doorClosedY), 0));
        doorPivot.Quaternion = targetRotation;
    }

    /// <summary>
    /// Animate the door opening / closing to the specified position.
    /// </summary>
    public virtual void Open(bool open)
    {
        isOpen = open;
        var targetRotation = Quaternion.FromEuler(new Vector3(0, Mathf.DegToRad(open ? doorOpenY : doorClosedY), 0));
        // TODO scale the tween duration if the door is partly opened. E.g. if you click on it while it's opening and
        // it's currently 20% open, then closing should only take 20% as long.
        Game.instance.GetTree().CreateTween().TweenProperty(doorPivot, "quaternion", targetRotation, OPEN_TWEEN_DURATION);
    }

    protected override void _HandleClick()
    {
        Open(!isOpen);
    }
}