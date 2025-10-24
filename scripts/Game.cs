using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MidnightBaking.scripts;

public partial class Game : Node
{
    public enum ItemId { APRON, BAKING_SODA, BROWN_SUGAR, LIGHT_SWITCH, SINK, SUGAR, TOWEL, TABLET, }
    public static Material flashMaterialOverlay => instance._flashMaterialOverlay;
    
    public static Game instance;
    public static readonly Dictionary<ItemId, Interactable> Interactables = new();
    private static readonly List<TaskGroup> tasks =
    [
        new TaskGroup(name: "Tutorial", tasks: [
            new Tasks.Group0_MouseLook(),
            new Tasks.Group0_Move(),
            new Tasks.Group0_Interact(),
        ]),
        new TaskGroup(name: "Initial prep", tasks: [
            new Tasks.Group1_WashHands(),
            new Tasks.Group1_PutOnApron(),
            new Tasks.Group1_OpenRecipe(),
        ]),
        new TaskGroup(name: "Gather ingredients", tasks: [
            
        ])
    ];

    [Export] public ChecklistManager checklistUi;
    [Export] public Material _flashMaterialOverlay;
    
    private static int currentTaskGroupIndex;

    public override void _Ready()
    {
        if (instance != null)
            throw new Exception("Duplicate Game instance. Should be exactly 1.");
        instance = this;

        GD.Print("Game._Ready()");
        GetTree().CreateTimer(1.0).Timeout += () =>
        {
            GD.Print($"Game - Registered interactables = {Interactables.Count}");
            StartTaskGroup(0);
        };
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

    private void OnCurrentTaskCompleted()
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
            task.Start(OnCurrentTaskCompleted);
        
        UpdateUi();
    }

    // TODO UpdateUi after settings changed
    public static void UpdateUi()
    {
        var taskGroup = tasks[currentTaskGroupIndex];
        
        foreach (Task task in taskGroup.tasks)
            GD.Print($"[{(task.complete ? "x" : " ")}] {task.description}");
        
        instance.checklistUi.Show(taskGroup);
    }

    public class TaskGroup(string name, List<Task> tasks)
    {
        /// <summary> The name is not meant for UI purposes, but for debugging. </summary>
        public readonly string name = name;
        public readonly List<Task> tasks = tasks;
    }

    public abstract class Task
    {
        public abstract string description { get; }
        public bool complete { get; private set; }
        public List<SubTask> subtasks = new();
        public List<string> hints = new();
        private Action onCompleteCallback;

        public void Start(Action onComplete)
        {
            GD.Print($"Task \"{description}\".Start()");
            complete = false;
            onCompleteCallback = onComplete;
            hints.Clear();
            OnStart();
            foreach (var subtask in subtasks)
                subtask.OnStart();
        }

        /// <summary>
        /// Mark this Task as completed, perform any OnComplete teardown functions, then notify the game.
        /// </summary>
        protected void Complete()
        {
            GD.Print($"Task \"{description}\".Complete()");
            complete = true;
            OnComplete();
            foreach (var subtask in subtasks)
                subtask.OnComplete();
            onCompleteCallback.Invoke();
        }

        /// <summary> Empty hook method. Override to perform setup functions upon Task start. </summary>
        protected virtual void OnStart() {}
        /// <summary> Empty hook method. Override to perform wrap up / teardown functions upon Task completion. </summary>
        protected virtual void OnComplete() {}
    }

    public abstract class SubTask
    {
        public string description;
        public bool complete;
        public List<string> hints = new();
        
        public virtual void OnStart() {}
        public virtual void OnInteract(ItemId item) {}
        public virtual void OnComplete() {}

    }
}