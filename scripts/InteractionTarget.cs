using Godot;

namespace MidnightBaking.scripts;

public partial class InteractionTarget : Node3D
{
    [Signal] public delegate void OnClickedEventHandler();
    /// <summary>
    /// Max distance from the collider that clicks will register. Useful for specifying
    /// where the player must be to interact with the object, given that different objects
    /// have different sized and shaped colliders in different positions.
    /// </summary>
    [Export] public float maxInteractionDistance = 1.0f;

    public void OnClick()
    {
        GD.Print($"{Name}.InteractionTarget.OnClick()");
        EmitSignalOnClicked();
    }
}