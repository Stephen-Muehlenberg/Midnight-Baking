using System.Collections.Generic;
using Godot;

namespace MidnightBaking.scripts;

public partial class ChecklistManager : VBoxContainer
{
    private List<ChecklistItem> items = new();

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
            items.Add(child as ChecklistItem);
    }

    private class Entry(string text, bool isSubtitle = false, bool isHint = false, bool isComplete = false)
    {
        public string text = text;
        public bool isSubtask = isSubtitle;
        public bool isHint = isHint;
        public bool isComplete = isComplete;
    }

    public void Show(Game.TaskGroup taskGroup)
    {
        var entries = MapTaskGroupToEntries(taskGroup);
        SetChecklistItemCount(entries.Count);
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            items[i].Show(entry.text, entry.isSubtask, entry.isHint, entry.isComplete);
        }
    }
    
    private static List<Entry> MapTaskGroupToEntries(Game.TaskGroup taskGroup)
    {
        List<Entry> entries = new();

        for (int i = 0; i < taskGroup.tasks.Count; i++)
        {
            var task = taskGroup.tasks[i];
            entries.Add(new Entry(task.description, isComplete: task.complete));

            for (int j = 0; j < task.hints.Count; j++)
            {
                // A hint is complete if the parent is complete, or if it's not the most recent hint.
                bool complete = task.complete || j < task.hints.Count - 1;
                entries.Add(new Entry(task.hints[j], isHint: true, isComplete: complete));
            }

            for (int k = 0; k < task.subtasks.Count; k++)
            {
                var subtask = task.subtasks[k];
                entries.Add(new Entry(subtask.description, isSubtitle: true, isComplete: task.complete || subtask.complete));
                for (int l = 0; l < subtask.hints.Count; l++)
                {
                    // A hint is complete if the parent is complete, or if it's not the most recent hint.
                    bool complete = task.complete || subtask.complete || l < subtask.hints.Count - 1;
                    entries.Add(new Entry(subtask.hints[l], isSubtitle: true, isHint: true, isComplete: complete));
                }
            }
        }
        
        return entries;
    }

    /// <summary>
    /// Grows or shrinks the ChecklistItem object pool to match the specified size.
    /// </summary>
    private void SetChecklistItemCount(int count)
    {
        int itemCountDifference = items.Count - count;
        
        // If there aren't enough ChecklistItem instances, create more.
        if (itemCountDifference < 0)
            for (int i = 0; i < -itemCountDifference; i++)
            {
                var newItem = items[0].Duplicate() as ChecklistItem;
                AddChild(newItem);
                items.Add(newItem);
            }

        // Hide any ChecklistItem instances which aren't needed.
        for (int i = 0; i < items.Count; i++)
        {
            if (i < count)
                items[i].Show();
            else
                items[i].Hide();
        }
    }

    /// <summary>
    /// Adjust the specified task's hint to display new text.
    /// Useful for tasks with multiple sub-steps.
    /// </summary>
    public void UpdateHint(int index, string newHint)
    {
        
    }

    /// <summary>
    /// Checks off the specified entry.
    /// </summary>
    // TODO maybe have the checkbox pulse larger, and when its at maximum size
    // it changes to checked. When its finished returning to normal size (like 1/2
    // a second) animate a strikethrough across the text, starting from the left.
    // The speed should be constant, so longer lines should take more time.
    public void MarkComplete(int index)
    {
        
    }

    /// <summary>
    /// Animate all checklist items being removed from screen.
    /// </summary>
    // TODO maybe the group scrolls up and fades out?
    public void FadeOutAll()
    {
        
    }
}