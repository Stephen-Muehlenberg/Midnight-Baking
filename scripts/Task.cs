using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MidnightBaking.scripts;

public abstract class Task
{
    public abstract string description { get; }
    public bool complete { get; private set; }
    public List<Subtask> subtasks = new();
    public List<string> hints = new();
    private Action onCompleteCallback;

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

    /// <summary>
    /// Invoked when this task is started.
    /// </summary>
    public void Start(Action onCompleteCallback)
    {
        this.onCompleteCallback = onCompleteCallback;
        SetState_TaskStart();
        foreach (var subtask in subtasks)
            subtask.Start(OnSubtaskComplete);
    }

    public void Complete() => Complete(true);
    /// <summary>
    /// Mark this Task as completed, perform any OnComplete teardown functions, then notify the game.
    /// </summary>
    /// <param name="invokeOnCompleteCallback">Optionally, skip the <see cref="onCompleteCallback"/>. Mostly
    /// used for debugging purposes.</param>
    public void Complete(bool invokeOnCompleteCallback)
    {
        GD.Print($"Task \"{description}\".Complete()");
        complete = true;
        SetState_TaskComplete();
        foreach (var subtask in subtasks)
            subtask.SetState_SubtaskComplete();
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

    /// <summary>
    /// Hook method for subclasses to override. Invoked during <see cref="Initialise()"/>. Used to set (or reset) to initial game starting state.
    /// </summary>
    protected virtual void SetState_GameState() {}
    /// <summary> Hook method for subclasses to override. Invoked during <see cref="Start()"/>. Used to set up task interactions. </summary>
    protected virtual void SetState_TaskStart() {}
    /// <summary> Hook method for subclasses to override. Invoked during <see cref="Complete()"/>. Used to set final object states and perform teardown logic. </summary>
    protected virtual void SetState_TaskComplete() {}
}