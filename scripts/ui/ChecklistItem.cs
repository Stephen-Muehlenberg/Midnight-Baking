using Godot;

namespace MidnightBaking.scripts.ui;

public partial class ChecklistItem : Control
{
    [Export] private Label textField;
    [Export] private ColorRect strikeThrough;
    
    private static readonly Color normalColor = new(1,1,1,1);
    private static readonly Color hintColor = new(0.66f, 0.66f, 0.66f, 0.75f);
    
    public void Show(string text, bool isSubTask, bool isHint, bool isComplete)
    {
        char checkbox = isComplete ? '☑' : '☐';
        textField.Text = $"{(isSubTask ? "        " : "")}{(isHint ? ' ' : checkbox)} {text}";

        textField.Modulate = isHint
            ? hintColor
            : normalColor;
        
        if (isComplete)
            strikeThrough.Show();
        else
            strikeThrough.Hide();
    }

    public void MarkComplete()
    {
        // TODO
    }
}