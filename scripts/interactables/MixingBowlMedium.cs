using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class MixingBowlMedium : Pickupable
{
    [Export] public Node3D positionBenchPrepArea { get; private set; }
    [Export] public Node3D sievePivot;
    [Export] public MeshInstance3D powderMesh { get; private set; }
    protected override Node3D[] GetLocations() => [positionBenchPrepArea];
    
    protected override void _ResetToGameStartState()
    {
        SetPowderLevel(0);
    }

    public void SetPowderLevel(float fraction)
    {
        if (fraction is < 0 or > 1)
            GD.PushWarning($"SetPowderLevel() was invoked with a fraction of {fraction}, but must be between 0 and 1.");

        // TODO exponential doesn't work well here. Need something more nuanced.
        float multiplier = Mathf.Pow(Mathf.Clamp(fraction, 0, 1), 2);
        powderMesh.Mesh.Set("radius", 0.09 * multiplier);
        powderMesh.Mesh.Set("height", 0.06 * multiplier);
    }
}