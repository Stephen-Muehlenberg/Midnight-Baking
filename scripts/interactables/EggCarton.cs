using System;
using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class EggCarton : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
    
    //   [Export] private Node3D lid;
  //  [Export] private Node3D egg1;
   // [Export] private Node3D egg2;
    
    protected override void _ResetToGameStartState()
    {
        SetLidOpen(false);
        SetEggsMissing(0);
    }

    public void SetLidOpen(bool open)
    {
   //     lid.RotationDegrees = new Vector3(open ? 110 : 0, 0, 0);
    }

    public void SetEggsMissing(int amount)
    {
        if (amount < 0 || amount > 2)
            throw new Exception($"SetEggsMissing() expected between 0 and 2 eggs, but {amount} was set.");

    //    egg1.Visible = amount < 1;
    //    egg2.Visible = amount < 2;
    }
}