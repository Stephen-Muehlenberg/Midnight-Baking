using System;
using System.Collections.Generic;
using Godot.Collections;

namespace MidnightBaking.scripts.tasks;

public class TaskGroup(string name, List<ParentTask> tasks)
{
    /// <summary> The name is not meant for UI purposes, but for debugging. </summary>
    public readonly string name = name;
    public readonly List<ParentTask> tasks = tasks;

    public void Start(Action OnTaskOrSubtaskComplete)
    {
        foreach (var task in tasks)
            task.Start(OnTaskOrSubtaskComplete);
    }

    public void Cancel()
    {
        foreach (var task in tasks)
            task.Cancel();
    }
        
    public void Save(Dictionary data)
    {
        Dictionary taskSaveData = new();
        foreach (var task in tasks)
            task.Save(taskSaveData);
        data["taskGroup"] = taskSaveData;
    }

    /// <summary>
    /// Assume <see cref="TaskGroup.Start"/> has already been called.
    /// </summary>
    public void Load(Dictionary data)
    {
        var taskSaveData = data["taskGroup"].AsGodotDictionary();
        foreach (var task in tasks)
            task.Load(taskSaveData);
    }
}