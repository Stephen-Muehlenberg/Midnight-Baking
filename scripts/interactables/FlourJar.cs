using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class FlourJar : Pickupable
{
    [Export] public ContainerLid Lid { get; private set; }
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
    private bool lidAttached = true;

    protected override void _ResetToGameStartState()
    {
        SetLidAttached(true);
    }

    public void SetLidAttached(bool attached)
    {
        if (lidAttached == attached) return;
        lidAttached = attached;
        
        // Make it so that the jar's highlight includes or excludes the lid depending on whether it is on or not.
        foreach (var mesh in Lid.highlightMeshes)
        {
            if (attached)
            {
                if (!highlightMeshes.Contains(mesh))
                    highlightMeshes.Add(mesh);
            }
            else highlightMeshes.Remove(mesh);
        }
        foreach (var csg in Lid.highlightCsgs)
        {
            if (attached)
            {
                if (!highlightCsgs.Contains(csg))
                    highlightCsgs.Add(csg);
            }
            else highlightCsgs.Remove(csg);
        }
    }
}