using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public partial class Game : Node
{
    public enum ItemId { APRON, BAKING_SODA, BROWN_SUGAR, LIGHT_SWITCH, SINK,
        SUGAR, TOWEL, TABLET, MICROWAVE, PANTRY_DOOR, SOAP, FRIDGE, EGG_CARTON, BENCH_PREP_AREA,
        BUTTER, CUPBOARD_UPPER_DOOR, FLOUR_JAR, VANILLA_JAR, CHOC_CHIP_BAG, OVEN }
    public static Material flashMaterialOverlay => instance._flashMaterialOverlay;
    
    public static Game instance;
    public static bool started = false;
    public static bool paused = false;
    public static readonly Dictionary<ItemId, Interactable> Interactables = new();
    private List<Tasks.TaskGroup> tasks;

    [Export] public Control pauseMenu;
    [Export] public ui.ChecklistManager checklistUi;
    [Export] public Material _flashMaterialOverlay;
    
    private static int currentTaskGroupIndex;

    public static Apron Apron => Interactables[ItemId.APRON] as Apron;
    public static Pickupable BakingSoda => Interactables[ItemId.BAKING_SODA] as Pickupable;
    public static BenchPrepArea BenchPrepArea => Interactables[ItemId.BENCH_PREP_AREA] as BenchPrepArea;
    public static Pickupable BrownSugarJar => Interactables[ItemId.BROWN_SUGAR] as Pickupable;
    public static Pickupable ChocChipBag => Interactables[ItemId.CHOC_CHIP_BAG] as Pickupable;
    public static Door CupboardUpper => Interactables[ItemId.CUPBOARD_UPPER_DOOR] as Door;
    public static Interactable Butter => Interactables[ItemId.BUTTER];
    public static EggCarton EggCarton => Interactables[ItemId.EGG_CARTON] as EggCarton;
    public static Fridge Fridge => Interactables[ItemId.FRIDGE] as Fridge;
    public static FlourJar FlourJar => Interactables[ItemId.FLOUR_JAR] as FlourJar;
    public static KitchenLightController LightSwitch => Interactables[ItemId.LIGHT_SWITCH] as KitchenLightController;
    public static Interactable Sink => Interactables[ItemId.SINK];
    public static Interactable Soap => Interactables[ItemId.SOAP];
    public static Interactable SugarJar => Interactables[ItemId.SUGAR];
    public static OvenController Oven;
    public static Tablet Tablet => Interactables[ItemId.TABLET] as Tablet;
    public static Interactable Towel => Interactables[ItemId.TOWEL];
    public static Pickupable VanillaJar => Interactables[ItemId.VANILLA_JAR] as Pickupable;

    public override void _Ready()
    {
        if (instance != null)
            throw new Exception("Duplicate Game instance. Should be exactly 1.");
        instance = this;
        
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
    }

    /// <summary>
    /// Adds Interactable instance to the dictionary.
    /// Should be invoked by all <see cref="Interactable"/>s during their <see cref="_Ready()"/>.
    /// </summary>
    public static void RegisterInteractable(Interactable interactable)
    {
        Interactables.Add(interactable.itemId, interactable);
        interactable.ResetToGameStartState();
    }

    public void StartGame()
    {
        started = true;
        
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
        checklistUi.Show();
        StartTaskGroup(0);
        
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        PlayerController.instance.SetEnabled(true);
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || !started) return;
        
        if (keyEvent.Keycode == Key.Escape)
        {
            if (started && !paused)
                ShowPauseMenu(true);
        }
        // Debug shortcuts
        else if (keyEvent.Keycode == Key.Right)
            SkipTask();
        else if (keyEvent.Keycode == Key.Down)
            SkipTaskGroup();
    }

    private void ShowPauseMenu(bool show)
    {
        paused = show;
        PlayerController.instance.SetEnabled(!show);
        pauseMenu.Visible = show;
        Input.SetMouseMode(show
            ? Input.MouseModeEnum.Visible
            : Input.MouseModeEnum.Captured);
        // TODO pause/resume in-game clock
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

    // TODO also handle UI updates when a subtask is completed.
    private void OnTaskOrSubtaskComplete()
    {
        var currentTasksGroup = tasks[currentTaskGroupIndex];
        if (currentTasksGroup.tasks.All(task => task.complete))
            StartTaskGroup(currentTaskGroupIndex + 1);
        UpdateUi();
    }

    private void SkipTask()
    {
        // Find the next incomplete task.
        var taskGroup = tasks[currentTaskGroupIndex];
        ParentTask currentTask = taskGroup.tasks[0];
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
        var taskGroup = tasks[currentTaskGroupIndex];
        foreach (var task in taskGroup.tasks)
            task.Complete();
        OnTaskOrSubtaskComplete();
    }

    public static void UpdateUi()
    {
        var taskGroup = instance.tasks[currentTaskGroupIndex];
        instance.checklistUi.Show(taskGroup);
    }
}