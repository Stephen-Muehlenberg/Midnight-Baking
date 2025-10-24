using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class KitchenLightController : Interactable
{
    /// <summary>Lights enabled when the kitchen light is on. Includes bounce lights etc.</summary>
    [Export] private Light3D[] onLights;
    /// <summary>Lights enabled when the kitchen light is off. Includes ambient minimal light etc.</summary>
    [Export] private Light3D[] offLights;
    [Export] private Node3D lightSwitch;
    
    private bool lightOn;

    protected override void _ResetToGameStartState()
    {
        SetLightOn(false);
    }

    public void OnLightSwitchPressed()
    {
        SetLightOn(!lightOn);
        onClickCallback?.Invoke();
    }

    private void SetLightOn(bool on)
    {
        lightOn = on;

        if (on)
        {
            foreach (var light in onLights)
                light.Show();
            foreach (var light in offLights)
                light.Hide();
        }
        else
        {
            foreach (var light in onLights)
                light.Hide();
            foreach (var light in offLights)
                light.Show();
        }
        
        lightSwitch.RotationDegrees = new Vector3(on ? -30 : 30, 0, 0);
    }
}