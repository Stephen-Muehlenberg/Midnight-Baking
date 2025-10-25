using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Microwave : Interactable
{
    [Export] private Label3D timeText;
    // It's tricky to disable the : character without affecting the position of the 
    // other characters, so it's just stored in a separate label which is toggled by itself.
    [Export] private Label3D colonText;

    private bool updateClockInRealtime = false;
    /// <summary> The number of <see cref="Time.GetTicksMsec()"/> when in-game midnight started. </summary>
    private float midnightStartTimeMs = float.NaN;
    
    public override void _Process(double delta)
    {
        bool isEvenSecond = Mathf.FloorToInt(Time.GetTicksMsec() / 1000f) % 2 == 0;
        colonText.Visible = isEvenSecond;

        // If we're not yet tracking real time, or we are but we're waiting for the current
        // second to tick over, then don't update the time display.
        if (!updateClockInRealtime || Time.GetTicksMsec() < midnightStartTimeMs)
            return;

        float ingameTimeMs = Time.GetTicksMsec() - midnightStartTimeMs;
        int ingameTimeM = Mathf.FloorToInt(ingameTimeMs / 60_000f);
        string minutes = (ingameTimeM % 60).ToString("00");
        int ingameTimeH = Mathf.FloorToInt(ingameTimeM / 60f);
        string hours = ingameTimeH < 1
            ? "12"
            : ingameTimeH.ToString("00");
        timeText.SetText($"{hours} {minutes}");
    }

    protected override void _ResetToGameStartState()
    {
        updateClockInRealtime = false;
        midnightStartTimeMs = float.NaN;
        timeText.SetText("11 59");
    }

    public void SetTimeToMidnightAndStartClock()
    {
        float seconds = Time.GetTicksMsec() / 1000f;
        float nextSecond = Mathf.Ceil(seconds);
        midnightStartTimeMs = nextSecond * 1000f;
        updateClockInRealtime = true;
    }
}