using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Sieve : Pickupable
{
    /// <summary>Equivalent to 2.5 cups + 1 teaspoon.</summary>
    private const float FULL_CUPS_AMOUNT = 2.52083333f;
    
    [Export] public Node3D positionInBowl { get; private set; }
    [Export] public MeshInstance3D powderMesh { get; private set; }
    [Export] public GpuParticles3D particlesFalling { get; private set; }
    [Export] public GpuParticles3D particlesCloud { get; private set; }
    protected override Node3D[] GetLocations() => [positionInBowl];

    protected override void _ResetToGameStartState()
    {
        particlesFalling.AmountRatio = 0;
        particlesCloud.AmountRatio = 0;
        SetPowderLevel_AsFraction(0);
    }
    
    public void SetPowderLevel_AsCups(float cups) => SetPowderLevel_AsFraction(cups / FULL_CUPS_AMOUNT);

    public void SetPowderLevel_AsFraction(float fullFraction)
    {
        if (fullFraction is < 0 or > 1)
            GD.PushWarning($"SetPowderLevel_AsFraction() was invoked with a fraction of {fullFraction}, but must be between 0 and 1.");
        
        // TODO exponential doesn't work well here. Need something more nuanced.
        float multiplier = Mathf.Pow(Mathf.Clamp(fullFraction, 0, 1), 2);
        powderMesh.Mesh.Set("radius", 0.09 * multiplier);
        powderMesh.Mesh.Set("height", 0.06 * multiplier);
        // TODO scale the width and amount of particle emissions based powder size.
        // Or are we already handling that via the "pressure" calculation in the shake controller?
    }
}