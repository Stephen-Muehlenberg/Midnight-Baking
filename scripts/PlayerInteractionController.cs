using System.Linq;
using Godot;
using Godot.Collections;

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
        // Ignore any held items.
        Array<Rid> excludedColliders = [];
        if (Player.heldItemLeft != null && Player.heldItemLeft == Player.heldItemRight)
        {
            var heldItemRids = Player.heldItemLeft
                .FindChildren("*", type: nameof(CollisionObject3D))
                .Select(it => ((CollisionObject3D)it).GetRid());
            excludedColliders = new Array<Rid>(heldItemRids);
        }
        
        // Perform raycast.
        Vector3 origin = camera.GlobalPosition;
        Vector3 offset = -camera.GlobalBasis.Z * 5f;
        Vector3 end = origin + offset;
        var query = PhysicsRayQueryParameters3D.Create(origin, end, 1, excludedColliders);
        query.CollideWithAreas = true;
        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

        // Check results for any InteractionTargets.
        if (result.Count == 0)
        {
            GD.Print($"PlayerInteractionController.Interact(): didn't click on anything...");
            return;
        }

        var collisionObject = result["collider"].Obj;
        if (collisionObject is InteractionTarget target)
        {
            var collisionPoint = (Vector3) result["position"].Obj;
            float distance = (collisionPoint - origin).Length();

            // Get some info for logging. Can be removed later.
            var parentInteractable = GetInteractableAncestor(target);
            string nameString = parentInteractable == null
                ? $"\"{target.Name}\" (collider object; no parent interactable found)"
                : $"\"{parentInteractable.Name}\"";
            GD.Print($"PlayerInteractionController: clicked on {nameString}, distance {distance}, max dist {target.maxInteractionDistance}");

            // Check if the object is within the collider's maximum interaction distance.
            if (distance <= target.maxInteractionDistance)
                target.OnClick();
        }
        else if (collisionObject is Node3D node)
            GD.Print($"PlayerInteractionController: clicked on non-interactive Node3D: {node.GetParent().Name}.{node.Name}");
        else
            GD.Print($"PlayerInteractionController: clicked on something but not sure what???");
    }

    // Used for logging. Can be removed later.
    private Interactable GetInteractableAncestor(Node node)
    {
        int recursionLevel = 0;
        while (true)
        {
            var parent = node.GetParent();
            if (parent == null) return null;
            if (parent is Interactable interactable) return interactable;
            if (recursionLevel >= 2) return null;
            node = parent;
            recursionLevel++;
        }
    }
}