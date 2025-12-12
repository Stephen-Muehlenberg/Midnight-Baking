using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Drawer : Interactable
{
    public bool IsOpen {get; private set;}

    protected override void _ResetToGameStartState()
    {
        SetOpen(false);
        SetCanInteract(true);
    }

    public void SetOpen(bool open)
    {
        IsOpen = open;
        
        Position = new Vector3(0, Position.Y, open ? 0.407f : 0.097f);
    }

    protected override void _HandleClick()
    {
        SetOpen(!IsOpen);
    }
}
