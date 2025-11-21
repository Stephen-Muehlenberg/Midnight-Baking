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
            SetState_GameState();
    }

    public override void Start(Action onCompleteCallback)
    {
        complete = false;
        hints.Clear();
        this.onCompleteCallback = onCompleteCallback;
        SetState_TaskStart();
        foreach (var subtask in subtasks)
            subtask.Start(OnSubtaskComplete);
    }

    public override void Complete(bool invokeOnCompleteCallback = true)
    {
        complete = true;
        SetState_TaskComplete();
        foreach (var subtask in subtasks)
            subtask.SetState_TaskComplete();
        if (invokeOnCompleteCallback)
            onCompleteCallback.Invoke();
    }

    private void OnSubtaskComplete()
    {
        if (subtasks.All(subtask => subtask.complete))
            Complete();
        else
            Game.UpdateUi();
    }
}