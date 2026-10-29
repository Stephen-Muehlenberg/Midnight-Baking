using System;
using Godot;
using Godot.Collections;

namespace MidnightBaking.scripts.interactables;

public partial class Butter : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
}