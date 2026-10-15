using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MidnightBaking.scripts;

public abstract class ParentTask : Task
{
    public List<Subtask> subtasks { get; } = new();

    /// <summary>
    /// Invoked when the game is set (or reset) to its initial state.
    /// </summary>
    public void Initialise()
    {
        complete = false;
        hints.Clear();
        SetState_GameState();
        foreach (Subtask subtask in subtasks)
            subtask.SetState_GameState();
    }

    public override void Start(Action onCompleteCallback)
    {
        complete = false;
        hints.Clear();
        this.onCompleteCallback += onCompleteCallback;
        SetState_TaskStart();
        foreach (var subtask in subtasks)
            subtask.Start(OnSubtaskComplete);
    }

    /// <summary>
    /// Adds the new subtask, and optionally starts it.
    /// <br/>
    /// <br/>If adding subtasks after <see cref="Start"/> has been called,
    /// prefer this method over directly modifying the subtask List, as this method
    /// explicitly requires you to choose whether to start to the subtask or not.
    /// Helps prevent adding the task but forgetting to start it.
    /// </summary>
    protected void AddSubtask(Subtask subtask, bool startSubtask)
    {
        subtasks.Add(subtask);
        if (startSubtask)
            subtask.Start(OnSubtaskComplete);
    }

    public override void Complete(bool invokeOnCompleteCallback = true)
    {
        complete = true;
        SetState_TaskComplete();
        foreach (var subtask in subtasks)
            subtask.SetState_TaskComplete();
        if (invokeOnCompleteCallback)
            onCompleteCallback_Invoke();
    }

    private void OnSubtaskComplete()
    {
        if (subtasks.All(subtask => subtask.complete))
            Complete();
        else
            Game.UpdateUi();
    }
}