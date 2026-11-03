using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class MixingBowlMedium : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    [Export] public Node3D sievePivot;
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
}