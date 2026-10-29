using System.Linq;
using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class FlourJar : Pickupable
{
    [Export] public FlourJarLid Lid { get; private set; }
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
    private bool lidAttached = true;

    public void SetLidAttached(bool attached)
    {
        if (lidAttached == attached) return;
        lidAttached = attached;
//        foreach (var mesh in Lid.highlightMeshes)
  //          highlightMeshes
//        highlightMeshes.
        // TODO include / remove lid mesh from highlight meshes, depending on whether lid is attached or not.
    } 
}