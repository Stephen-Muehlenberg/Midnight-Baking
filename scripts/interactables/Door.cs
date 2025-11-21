using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Door : Interactable
{
    [Export] private Node3D doorPivot;
    [Export] private float doorClosedY = 0;
    [Export] private float doorOpenY = 90;
    
    public bool isOpen {get; private set;}
    
    protected override void _ResetToGameStartState()
    {
        SetOpen(false);
        SetCanInteract(true);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        doorPivot.RotationDegrees = new Vector3(0, open ? doorOpenY : doorClosedY, 0);
    }

    protected override void _HandleClick()
    {
        SetOpen(!isOpen);
    }
}