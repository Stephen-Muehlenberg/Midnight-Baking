using Godot;

namespace MidnightBaking.scripts;

public partial class OvenController : Node3D, Resetable
{
    [Export] public Interactable Door;
    [Export] public Interactable TemperatureDial;
    [Export] private Light3D light;
    
    public bool doorOpen { get; private set; }

    public override void _Ready()
    {
        Door.AddClickListener(OnDoorClicked, highlight: false);
        Game.Oven = this;
    }

    public void ResetToGameStartState()
    {
        SetOn(false);
        SetOpen(false);
    }

    public void SetOn(bool on)
    {
        light.Visible = on;
        TemperatureDial.RotationDegrees = new Vector3(0, 0, on ? 90 : 0);
        // TODO play oven fan sound? very gently?
    }

    public void SetOpen(bool open)
    {
        doorOpen = open;
        Door.RotationDegrees = new Vector3(open ? 80 : 0, 0, 0);
    }

    private void OnDoorClicked() => SetOpen(!doorOpen);
}