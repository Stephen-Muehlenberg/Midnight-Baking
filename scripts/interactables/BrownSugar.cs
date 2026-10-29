using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class BrownSugar : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
}