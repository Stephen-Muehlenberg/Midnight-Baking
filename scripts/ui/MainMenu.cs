using Godot;

namespace MidnightBaking.scripts.ui;

public partial class MainMenu : Control
{
    private const float FADE_IN_DURATION = 1.5f;
    
    [Export] private ColorRect blackFadeFill;
    [Export] private Node3D title;

    private bool fadingIn = true;
    private float fadeTime = 0;
    
    public override void _Ready()
    {
        // Fade in audio
        // Set player position?
        // Fade in menu
        // Set focal length to blur out everything beyond like 1m?
    }

    public override void _Process(double delta)
    {
        if (!fadingIn) return;
        
        fadeTime += (float) delta;
        if (fadeTime >= FADE_IN_DURATION)
        {
            fadingIn = false;
            blackFadeFill.Hide();
        }
        else
        {
            float fadeFraction = fadeTime / FADE_IN_DURATION;
            blackFadeFill.Color = new Color(0,0,0, 1-fadeFraction);
        }
    }
    
    // This is just temporary debugging shortcuts. Can remove later.
    public override void _Input(InputEvent @event)
    {
        if (!Visible) return;
        
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed) return;

        if (keyEvent.Keycode == Key.Equal)
            OnStartClicked();
        else if (keyEvent.Keycode == Key.Q)
            GetTree().Quit();
    }

    public void OnStartClicked()
    {
        title.Hide();
        Hide();
        Game.instance.StartGame();
        // Animate camera to player position?
        // Change focus to bring blurred stuff back into focus?
    }

    public void OnMenuClicked()
    {
        // TODO
    }

    public void OnExitClicked()
    {
        GetTree().Quit();
    }
}