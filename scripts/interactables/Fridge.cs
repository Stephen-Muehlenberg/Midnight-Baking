using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Fridge : Interactable
{
    [Export] private Light3D light;
    [Export] private Node3D doorPivot;
    // TODO have the frdige beep if the door is left open for too long
    
    public bool isOpen {get; private set;}
    
    protected override void _ResetToGameStartState()
    {
        SetOpen(false);
        SetCanInteract(true);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        doorPivot.RotationDegrees = new Vector3(0, open ? -105 : 0, 0);
        light.Visible = open;
    }

    protected override void HandleClick()
    {
        SetOpen(!isOpen);
    }
}