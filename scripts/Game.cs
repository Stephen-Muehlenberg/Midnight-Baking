using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public partial class Game : Node
{
    public enum ItemId { APRON, BAKING_SODA, BROWN_SUGAR, LIGHT_SWITCH, SINK,
        SUGAR, TOWEL, TABLET, MICROWAVE, PANTRY_DOOR, SOAP, FRIDGE }
    public static Material flashMaterialOverlay => instance._flashMaterialOverlay;
    
    public static Game instance;
    public static readonly Dictionary<ItemId, Interactable> Interactables = new();
    private List<TaskGroup> tasks;

    [Export] public ChecklistManager checklistUi;
    [Export] public Material _flashMaterialOverlay;
    
    private static int currentTaskGroupIndex;

    public override void _Ready()
    {
        if (instance != null)
            throw new Exception("Duplicate Game instance. Should be exactly 1.");
        instance = this;
        
        // Wait 1/10th of a second before starting so that the various interactables can
        // register themselves.
        // This hack will be removed once we set up a main menu and fade in from black effect.
        GetTree().CreateTimer(0.1).Timeout += StartGame;
    }

    public void StartGame()
    {
        // Reset interactable objects.
        foreach (var interactable in Interactables.Values)
            interactable.ResetToGameStartState();
        
        // Get (or reset) list of tasks, then initialise it.
        tasks = Tasks.CreateTaskList();
        foreach (var taskGroup in tasks)
            foreach (var task in taskGroup.tasks)
                task.Initialise();
        
        // Set the in-game time to midnight and start the clocks.
        (Interactables[ItemId.MICROWAVE] as Microwave).SetTimeToMidnightAndStartClock();
        
        // Begin the first set of tasks.
        StartTaskGroup(0);
    }
    
    // Listen to key presses for debugging / testing purposes.
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.Right && keyEvent.Pressed)
                SkipTask();
            else if (keyEvent.Keycode == Key.Down && keyEvent.Pressed)
                SkipTaskGroup();
        }
    }

    private void SkipTask()
    {
        // Find the next incomplete task.
        var taskGroup = tasks[currentTaskGroupIndex];
        Task currentTask = taskGroup.tasks[0];
        for (int i = 0; i < taskGroup.tasks.Count; i++)
        {
            var task = taskGroup.tasks[i];
            if (!task.complete)
            {
                currentTask = task;
                break;
            }
        }
        
        // If the task has subtasks, find the next incomplete subtask, and set it to complete.
        if (currentTask.subtasks.Count > 0)
        {
            for (int i = 0; i < currentTask.subtasks.Count; i++)
            {
                var subtask = currentTask.subtasks[i];
                if (subtask.complete) continue;
                subtask.Complete();
                return;
            }
        }
        
        // If it has no subtasks, set the task itself to complete.
        currentTask.Complete();
    }

    private void SkipTaskGroup()
    {
        
    }

    /// <summary>
    /// Adds Interactable instance to the dictionary.
    /// Should be invoked by all <see cref="Interactable"/>s during their <see cref="_Ready"/>.
    /// </summary>
    public static void RegisterInteractable(Interactable interactable)
    {
        Interactables.Add(interactable.itemId, interactable);
        interactable.ResetToGameStartState();
    }

    // TODO also handle UI updates when a subtask is completed.
    private void OnTaskOrSubtaskComplete()
    {
        var currentTasksGroup = tasks[currentTaskGroupIndex];
        if (currentTasksGroup.tasks.All(task => task.complete))
            StartTaskGroup(currentTaskGroupIndex + 1);
        UpdateUi();
    }

    private void StartTaskGroup(int index)
    {
        currentTaskGroupIndex = index;
        if (index >= tasks.Count)
        {
            // TODO handle game completion
            GD.Print($"Tried to start task group #{index}, but there's only {tasks.Count} task groups. Game finished?");
            return;
        }
        
        var newTaskGroup = tasks[index];
        foreach (var task in newTaskGroup.tasks)
            task.Start(OnTaskOrSubtaskComplete);
        
        UpdateUi();
    }

    public static void UpdateUi()
    {
        var taskGroup = instance.tasks[currentTaskGroupIndex];
        instance.checklistUi.Show(taskGroup);
    }

    public class TaskGroup(string name, List<Task> tasks)
    {
        /// <summary> The name is not meant for UI purposes, but for debugging. </summary>
        public readonly string name = name;
        public readonly List<Task> tasks = tasks;
    }
}