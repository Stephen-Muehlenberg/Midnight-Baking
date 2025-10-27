using System;
using System.Collections.Generic;
using Godot;

namespace MidnightBaking.scripts;

public abstract class Subtask
{
    public abstract string description { get; }
    public bool complete { get; private set; }
    public List<string> hints = new();
    private Action onCompleteCallback;

    public void Start(Action onComplete)
    {
        GD.Print($"Subtask \"{description}\".Start()");
        complete = false;
        onCompleteCallback = onComplete;
        hints.Clear();
        SetState_SubtaskStart();
    }

    /// <summary>
    /// Mark this Subtask as completed, perform any OnComplete teardown functions, then notify the game.
    /// </summary>
    /// <param name="invokeOnCompleteCallback">Optionally, skip the <see cref="onCompleteCallback"/>. Mostly
    /// used for debugging purposes.</param>
    public void Complete(bool invokeOnCompleteCallback = true)
    {
        GD.Print($"Task \"{description}\".Complete()");
        complete = true;
        SetState_SubtaskComplete();
        if (invokeOnCompleteCallback)
            onCompleteCallback.Invoke();
    }
    
    public virtual void SetState_GameStart() {}
    public virtual void SetState_SubtaskStart() {}
    public virtual void SetState_SubtaskComplete() {}
}