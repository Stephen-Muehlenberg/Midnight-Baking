using System;

namespace MidnightBaking.scripts.tasks;

public abstract class Subtask : Task
{
    public override void Start(Action onCompleteCallback)
    {
        complete = false;
        hints.Clear();
        SetState_TaskStart();
        this.onCompleteCallback += onCompleteCallback;
    }

    public override void Complete(bool invokeOnCompleteCallback)
    {
        complete = true;
        DoTeardown();
        SetState_TaskComplete();
        if (invokeOnCompleteCallback)
            onCompleteCallback_Invoke();
    }
}