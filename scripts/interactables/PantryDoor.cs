using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class PantryDoor : Interactable
{
    [Export] private Node3D doorPivot;
    
    public bool isOpen {get; private set;}
    
    protected override void _ResetToGameStartState()
    {
        SetOpen(false);
        SetCanInteract(true);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        doorPivot.RotationDegrees = new Vector3(0, open ? 90 : 0, 0);
    }

    protected override void HandleClick()
    {
        SetOpen(!isOpen);
    }
}