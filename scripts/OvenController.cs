using System;
using Godot;

namespace MidnightBaking.scripts;

public partial class OvenController : Node3D, Resetable
{
    private const float DIAL_OFF_ROTATION = 0;
    private const float DIAL_ON_ROTATION = -160;
    
    [Export] public Interactable Door;
    [Export] public Interactable TemperatureDial;
    [Export] private Light3D light;

    public bool doorOpen { get; private set; }

    public override void _Ready()
    {
        Door.AddClickListener(OnDoorClicked, highlight: false);
        Items.Oven = this;
    }

    public void ResetToGameStartState()
    {
        SetOn(false);
        SetOpen(false);
    }

    public void SetOn(bool on)
    {
        light.Visible = on;
        TemperatureDial.RotationDegrees = new Vector3(0, 0, on ? DIAL_ON_ROTATION : DIAL_OFF_ROTATION);
        // TODO play oven fan sound? very gently?
    }

    public void AnimateOn(Action onAnimationEndCallback)
    {
        var tween = Game.instance.GetTree().CreateTween();
        tween.SetParallel(false);
        tween.TweenProperty(TemperatureDial, "rotation_degrees", new Vector3(0, 0, DIAL_ON_ROTATION), 0.4f);
        tween.TweenProperty(light, "visible", true, 0);
        tween.TweenCallback(Callable.From(onAnimationEndCallback));
        // TODO play oven fan sound? very gently?
    }

    public void SetOpen(bool open)
    {
        doorOpen = open;
        Door.RotationDegrees = new Vector3(open ? 80 : 0, 0, 0);
    }

    private void OnDoorClicked() => SetOpen(!doorOpen);
}