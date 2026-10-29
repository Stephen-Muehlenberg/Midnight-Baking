using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

namespace MidnightBaking.scripts.tasks;

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
        SetState_GameStart();
        foreach (Subtask subtask in subtasks)
            subtask.SetState_GameStart();
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

    public void Cancel()
    {
        ClearOnCompleteCallbacks();
        DoTeardown();
        foreach (var subtask in subtasks)
            subtask.DoTeardown();
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

    public override void Complete(bool invokeOnCompleteCallback)
    {
        complete = true;
        DoTeardown();
        SetState_TaskComplete();
        foreach (var subtask in subtasks)
        {
            subtask.DoTeardown();
            subtask.SetState_TaskComplete();
        }
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

    public override void Save(Dictionary data)
    {
        Dictionary parentTaskSaveData = new();
        foreach (var subtask in subtasks)
            subtask.Save(data);
//        data[]
    }

    public override void Load(Dictionary data)
    {
        foreach (var subtask in subtasks)
            subtask.Load(data);
    }
}