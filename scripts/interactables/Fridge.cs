using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Fridge : Door
{
    private const float DOOR_OPEN_BEEP_TIME = 30;
    
    [Export] private Light3D light;
    [Export] private AudioStreamPlayer3D beepAudioPlayer;

    private SceneTreeTimer beepTimer;

    public override void SetOpen(bool open)
    {
        base.SetOpen(open);
        light.Visible = open;
    }

    public override void Open(bool open)
    {
        isOpen = open;
        var targetRotation = Quaternion.FromEuler(new Vector3(0, Mathf.DegToRad(open ? doorOpenY : doorClosedY), 0));
        // TODO scale the tween duration if the door is partly opened. E.g. if you click on it while it's opening and
        // it's currently 20% open, then closing should only take 20% as long.
        var tween = Game.instance.GetTree().CreateTween();
        tween.SetParallel(false);
        tween.TweenProperty(doorPivot, "quaternion", targetRotation, OPEN_TWEEN_DURATION);
        if (open) // If opening the door, turn light on immediately.
            light.Visible = true;
        else // If closing the door, turn light off once the door is closed.
            tween.TweenCallback(Callable.From(OnDoorFullyClosed));

        if (open && beepTimer == null)
        {
            beepTimer = Game.instance.GetTree().CreateTimer(DOOR_OPEN_BEEP_TIME);
            beepTimer.Timeout += OnBeepTimerTriggered;
        }
    }

    private void OnDoorFullyClosed()
    {
        light.Visible = false;
        if (beepTimer == null) return;
        beepTimer.Timeout -= OnBeepTimerTriggered;
        beepTimer = null;
    }

    private void OnBeepTimerTriggered()
    {
        beepAudioPlayer.Play();
        beepTimer = Game.instance.GetTree().CreateTimer(DOOR_OPEN_BEEP_TIME);
        beepTimer.Timeout += OnBeepTimerTriggered;
    }
}