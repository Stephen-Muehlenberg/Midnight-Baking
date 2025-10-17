using Godot;
using ScarySub.scripts;

namespace MidnightBaking.scripts;

/// <summary>
/// Manages player interactions which occur when the player looks at an object,
/// or clicks an object they're looking at.
/// </summary>
public partial class PlayerLookController : Node3D
{
    [Export] private Camera3D camera;
    private float MAX_LOOK_DISTANCE = 10f; //TODO

    public override void _Input(InputEvent @event)
    {
        // Only listen for left mouse click events.
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
                Interact();
        }
    }

    /// <summary>
    /// Invoked when the user left mouse clicks. Performs the raycast
    /// and invokes the OnClick interaction of any InteractionTargets.
    /// </summary>
    private void Interact()
    {
        GD.Print("Interact()");
        // Perform raycast.
        Vector3 origin = camera.GlobalPosition;
        Vector3 offset = -camera.GlobalBasis.Z * MAX_LOOK_DISTANCE;
        Vector3 end = origin + offset;
        var query = PhysicsRayQueryParameters3D.Create(origin, end);
        query.CollideWithAreas = true;
        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

        // Check results for any InteractionTargets.
        if (result.Count == 0)
        {
            GD.Print($"LookController.Interact(): didn't click on anything...");
            return;
        }

        var collisionObject = result["collider"].Obj;
        if (collisionObject is InteractionTarget target)
        {
            GD.Print($"LookController.Interact(): clicked on an INTERACTION TARGET: {target.Name}");
            target.OnClick();
        }
        else if (collisionObject is Node3D node)
            GD.Print($"LookController.Interact(): clicked on non-interactive Node3D: {node.Name}");
        else
            GD.Print($"LookController.Interact(): clicked on something but not sure what???");
    }
}