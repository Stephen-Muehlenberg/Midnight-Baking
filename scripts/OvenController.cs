using Godot;

namespace MidnightBaking.scripts;

public partial class OvenController : Node3D
{
    [Export] public Interactable door;
    [Export] public Interactable temperatureKnob;
    [Export] private Light3D light;

    public override void _Ready()
    {
        Game.Oven = this;
        
    }

    protected override void _ResetToGameStartState()
    {
        SetOn(false);
        SetOpen(false);
    }

    public void SetOn(bool on)
    {
        light.Visible = on;
    }

    public void SetOpen(bool open)
    {
        // TODO
    }
}