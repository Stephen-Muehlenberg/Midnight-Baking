using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Sieve : Pickupable
{
    [Export] public Node3D positionInBowl { get; private set; }
    protected override Node3D[] GetLocations() => [positionInBowl];

    public void SetPowderLevel(float cups)
    {
        // TODO
    }
}