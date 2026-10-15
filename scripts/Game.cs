using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public partial class Game : Node
{
    public static Material flashMaterialOverlay => instance._flashMaterialOverlay;
    
    public static Game instance;
    public static bool started = false;
    public static bool paused = false;
    private List<Tasks.TaskGroup> tasks;

    [Export] public Control pauseMenu;
    [Export] public ui.ChecklistManager checklistUi;
    [Export] public Material _flashMaterialOverlay;
    [Export] private Node3D[] _locations;
    
    private static int currentTaskGroupIndex;
    
    public override void _Ready()
    {
        if (instance != null)
            throw new Exception("Duplicate Game instance. Should be exactly 1.");
        instance = this;
        
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
    }

    public class GameState
    {
        public bool lightSwitch_on;
        public bool apron_worn;
        public Location bakingSoda_location;
        public bool bakingSoda_initialVolume;

        public string ToJson()
        {
            Dictionary test = new();
//            test.Add("LightSwitch_On", lightSwitch_on);
            Item.Apron.SaveState(test);
            return Json.Stringify(test);
        }
    }

    public void LoadState(GameState state)
    {
        
    }

    private void TestRestart() // TODO delete this
    {
        Item.BakingSoda.SetLocation(Location.BakingSoda_Cupboard);
    }

    public void StartGame()
    {
        started = true;
        

        GameState test = new();
        var json = test.ToJson();
        Debug.Print($"test json: {json}");
        
        
        // Reset interactable objects.
        foreach (var item in Item.All.Values)
            item.ResetToGameStartState();
        Item.Oven.ResetToGameStartState();
        
        // Get (or reset) list of tasks, then initialise it.
        tasks = Tasks.CreateTaskList();
        foreach (var taskGroup in tasks)
            foreach (var task in taskGroup.tasks)
                task.Initialise();
        
        // Set the in-game time to midnight and start the clocks.
        Item.Microwave.SetTimeToMidnightAndStartClock();
        
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
        else if (keyEvent.Keycode == Key.Q)
            GetTree().Quit();
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