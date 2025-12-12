using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class BenchPrepArea : Interactable
{
    [Export] public Node3D bakingSodaPlacement;
    [Export] public Node3D brownSugarPlacement;
    [Export] public Node3D butterPlacement;
    [Export] public Node3D chocChipsPlacement;
    [Export] public Node3D eggCartonPlacement;
    [Export] public Node3D flourPlacement;
    [Export] public Node3D sugarPlacement;
    [Export] public Node3D vanillaPlacement;
    
    [Export] public Node3D mixingBowlPlacement;
    [Export] public Node3D measuringCupOnePlacement;
    [Export] public Node3D measuringCupHalfPlacement;
    [Export] public Node3D measuringSpoonPlacement;
}