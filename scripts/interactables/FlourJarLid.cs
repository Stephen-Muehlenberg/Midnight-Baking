using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class FlourJarLid : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];

    public override void SetPlacement(Node3D newParent)
    {
        base.SetPlacement(newParent);
        Items.FlourJar.SetLidAttached(newParent == initialPosition);
    }
}