using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class MeasuringCupHalf : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
}