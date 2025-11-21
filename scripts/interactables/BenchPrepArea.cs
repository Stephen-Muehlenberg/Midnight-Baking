using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class BenchPrepArea : Interactable
{
    [Export] public Node3D eggCartonPlacement;
    [Export] public Node3D butterPlacement;
    [Export] public Node3D flourPlacement;
}