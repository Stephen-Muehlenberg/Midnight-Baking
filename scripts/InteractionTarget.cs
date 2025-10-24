using Godot;

namespace MidnightBaking.scripts;

public partial class InteractionTarget : Node3D
{
    [Signal] public delegate void OnClickedEventHandler();

    public void OnClick()
    {
        GD.Print($"{Name}.InteractionTarget.OnClick()");
        EmitSignalOnClicked();
    }
}