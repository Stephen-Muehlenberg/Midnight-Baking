using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using MidnightBaking.scripts.tasks;

namespace MidnightBaking.scripts;

public partial class Game : Node
{
    public static Material flashMaterialOverlay => instance._flashMaterialOverlay;
    
    public static Game instance;
    public static bool started = false;
    public static bool paused = false;
    private List<TaskGroup> tasks;

    [Export] public PlayerController playerController;
    [Export] public Control pauseMenu;
    [Export] public ui.ChecklistManager checklistUi;
    [Export] public Material _flashMaterialOverlay;
    [Export] private Node3D[] _locations;
    
    private static int currentTaskGroupIndex = -1;
    
    public override void _Ready()
    {
        if (instance != null)
            throw new Exception("Duplicate Game instance. Should be exactly 1.");
        instance = this;
        
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
    }

    public void StartGame()
    {
        started = true;
        
        ResetItemsAndTasks();
        
        // Begin the first set of tasks.
        checklistUi.Show();
        StartTaskGroup(0);
        
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        PlayerController.instance.SetEnabled(true);
    }

    private void ResetItemsAndTasks()
    {
        // Reset interactable objects.
        foreach (var item in Items.All.Values)
            item.ResetToGameStartState();
        Items.Oven.ResetToGameStartState();
        
        // Get (or reset) list of tasks, then initialise it.
        tasks = Tasks.CreateTaskList();
        foreach (var taskGroup in tasks)
        foreach (var task in taskGroup.tasks)
            task.Initialise();
        
        // Set the in-game time to midnight and start the clocks.
        Items.Microwave.SetTimeToMidnightAndStartClock();
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
        else if (keyEvent.Keycode == Key.Minus)
            SkipTask();
        else if (keyEvent.Keycode == Key.Equal)
            SkipTaskGroup();
        else if (keyEvent.Keycode == Key.Q)
            GetTree().Quit();
        else if (keyEvent.Keycode == Key.F6)
            SaveGame();
        else if (keyEvent.Keycode == Key.F9)
            LoadGame();
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
        Debug.Print($"StartTaskGroup({index})");
        currentTaskGroupIndex = index;
        if (index >= tasks.Count)
        {
            // TODO handle game completion
            Debug.Print("ALL TASKS COMPLETED!!!");
            return;
        }
        
        tasks[index].Start(OnTaskOrSubtaskComplete);
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

    public void SaveGame()
    {
        // Gather game state data.
        Dictionary data = new();
        data["task"] = currentTaskGroupIndex;
        tasks[currentTaskGroupIndex].Save(data);
        Dictionary playerData = new();
        playerController.Save(playerData);
        data["player"] = playerData;
        var ids = Enum.GetNames(typeof(ItemId));
        for (int i = 1; i < ids.Length; i++) // Skip first id, which is deliberately null.
        {
            ItemId idEnum = (ItemId) i;
            if (!Items.All.ContainsKey(idEnum))
                continue;
            string id = ids[i];
            var interactable = Items.All[idEnum];
            Dictionary itemData = new();
            interactable.Save(itemData);
            if (itemData.Count > 0)
                data[id] = itemData;
        }
        
        // Serialize to string.
        string json = Json.Stringify(data);
        Debug.Print($"SAVING...:\n{json}");
        
        // Write to disk.
        using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
        saveFile.StorePascalString(json);
    }

    public void LoadGame()
    {
        // Read saved string from disk.
        if (!FileAccess.FileExists("user://savegame.save"))
            return; // Error! We don't have a save to load.
        using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);
        var json = saveFile.GetPascalString();
        Debug.Print($"LOADING...:\n{json}");
        
        // Parse string into dictionary.
        Dictionary data = (Dictionary) Json.ParseString(json);
        
        ResetItemsAndTasks();
        
        // Apply state to game.
        if (currentTaskGroupIndex >= 0) // If we've already started a TaskGroup, then exit out of it.
            tasks[currentTaskGroupIndex].Cancel();
        StartTaskGroup(data["task"].AsInt32());
        tasks[currentTaskGroupIndex].Load(data);
        var playerData = (Dictionary) data["player"];
        Debug.Print($"player data: {playerData}");
        playerController.Load((Dictionary) data["player"]);
        var ids = Enum.GetNames(typeof(ItemId));
        for (int i = 1; i < ids.Length; i++) // Skip first id, which is deliberately null.
        {
            ItemId idEnum = (ItemId) i;
            if (!Items.All.ContainsKey(idEnum))
                continue;
            string id = ids[i];
            if (!data.ContainsKey(id))
                continue;
            var interactable = Items.All[idEnum];
            Dictionary itemData = (Dictionary) data[id];
            interactable.Load(itemData);
        }
    }
}