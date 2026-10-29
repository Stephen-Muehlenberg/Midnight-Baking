using System;
using System.Collections.Generic;
using Godot.Collections;

namespace MidnightBaking.scripts.tasks;

public abstract class Task
{
    public abstract string description { get; }
    public bool complete { get; protected set; }
    public List<string> hints = new();
    public event Action onCompleteCallback;
    /// <summary>Workaround to the annoying limitation that children can't invoke a parent's event.</summary>
    protected void onCompleteCallback_Invoke() => onCompleteCallback?.Invoke();

    /// <summary>
    /// Invoked when this task is started.
    /// </summary>
    public abstract void Start(Action onCompleteCallback);

    protected void ClearOnCompleteCallbacks()
    {
        onCompleteCallback = null;
    }

    /// <summary>
    /// Mark this Task as completed, perform any OnComplete teardown functions, then notify the game.
    /// </summary>
    /// <param name="invokeOnCompleteCallback">Optionally, skip the <see cref="onCompleteCallback"/>.
    /// Defaults to true. Mostly used for debugging purposes.</param>
    public abstract void Complete(bool invokeOnCompleteCallback);
    
    /// <summary>
    /// Mark this Task as completed, perform any OnComplete teardown functions, then notify the game.
    /// </summary>
    // An argument-less variant of the Complete() method so it can be used in Actions.
    public void Complete() => Complete(true);
    
    /// <summary>
    /// Hook method for subclasses to override. Invoked during <see cref="Initialise()"/>. Used to set (or reset) to initial game starting state.
    /// </summary>
    public virtual void SetState_GameStart() {}
    
    /// <summary> Hook method for subclasses to override. Invoked during <see cref="Start()"/>. Used to set up task interactions. </summary>
    protected virtual void SetState_TaskStart() {}

    /// <summary> Hook method for subclasses to override. Invoked during <see cref="Complete()"/>. Used to set final object states. Should also call <see cref="SetState_TaskEnd"/>. </summary>
    public virtual void SetState_TaskComplete() {}

    /// <summary> Optional hook method for subclasses to override. </summary>
    public virtual void Save(Dictionary data) {}
    
    /// <summary> Optional hook method for subclasses to override. </summary>
    public virtual void Load(Dictionary data) {}

    public virtual void DoTeardown() {}

    /// <summary>
    /// Syntactic sugar, equivalent to calling <see cref="Interactable.AddClickListener()"/>.
    /// <br/>Provides optional extra debug information automatically.
    /// </summary>
    protected void AddClickListener(Interactable interactable, Action callback, bool highlight = true, Interactable.Requirement? requirementOverride = null)
        => interactable.AddClickListener(callback, highlight, requirementOverride, GetType().Name);

    /// <summary>
    /// Syntactic sugar, equivalent to calling <see cref="Interactable.AddClickListener()"/>.
    /// <br/>Provides optional extra debug information automatically.
    /// </summary>
    protected void AddClickListener(Interactable interactable, Action callback, Func<bool> highlight, Interactable.Requirement? requirementOverride = null)
        => interactable.AddClickListener(callback, highlight, requirementOverride, GetType().Name);
}