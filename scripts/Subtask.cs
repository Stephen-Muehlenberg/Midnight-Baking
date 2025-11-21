using System;
using System.Collections.Generic;
using Godot;

namespace MidnightBaking.scripts;

public abstract class Subtask : Task
{
    public override void Start(Action onCompleteCallback)
    {
        complete = false;
        hints.Clear();
        SetState_TaskStart();
        this.onCompleteCallback = onCompleteCallback;
    }
    
    public override void Complete(bool invokeOnCompleteCallback = true)
    {
        complete = true;
        SetState_TaskComplete();
        if (invokeOnCompleteCallback)
            onCompleteCallback.Invoke();
    }
}