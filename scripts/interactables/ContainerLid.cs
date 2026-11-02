using Godot;

namespace MidnightBaking.scripts.interactables;

/// <summary>
/// A simple pickupable with exactly two positions: attached/on its container (initial position),
/// and set aside on the bench.
/// </summary>
public partial class ContainerLid : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
}