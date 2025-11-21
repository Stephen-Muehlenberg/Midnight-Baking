using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Tablet : Interactable
{
    [Export] private Node3D screenOff;
    [Export] private Node3D screenOn;
    [Export] private Node3D text;

    public bool on { get; private set; }
    
    protected override void _ResetToGameStartState()
    {
        SetTabletOn(false);
    }

    protected override void _HandleClick()
    {
        SetTabletOn(!on);
    }

    public void SetTabletOn(bool on)
    {
        this.on = on;

        if (on)
        {
            screenOff.Hide();
            screenOn.Show();
            text.Show();
        }
        else
        {
            screenOff.Show();
            screenOn.Hide();
            text.Hide();
        }
    }
}