using Godot;

namespace MidnightBaking.scripts;

/// <summary>
/// Manages player interactions which occur when the player clicks
/// an object they're looking at.
/// </summary>
public partial class PlayerInteractionController : Node3D
{
    [Export] private Camera3D camera;

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
        // Perform raycast.
        Vector3 origin = camera.GlobalPosition;
        Vector3 offset = -camera.GlobalBasis.Z * 5f;
        Vector3 end = origin + offset;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, 1);
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
            
            // Check if the object is within the collider's maximum interaction distance.
            var collisionPoint = (Vector3) result["position"].Obj;
            float distance = (collisionPoint - origin).Length();
            GD.Print($"- object distance is {distance}; max distance is {target.maxInteractionDistance}");
            if (distance <= target.maxInteractionDistance)
            {
                target.OnClick();
            }
        }
        else if (collisionObject is Node3D node)
            GD.Print($"LookController.Interact(): clicked on non-interactive Node3D: {node.GetParent().Name}.{node.Name}");
        else
            GD.Print($"LookController.Interact(): clicked on something but not sure what???");
    }
}