namespace MidnightBaking.scripts.interactables;

public partial class Soap : Interactable
{
    protected override void _ResetToGameStartState()
    {
        SetCanInteract(false);
    }
}