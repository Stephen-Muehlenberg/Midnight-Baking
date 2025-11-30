using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public static class Tasks
{
    public class TaskGroup(string name, List<ParentTask> tasks)
    {
        /// <summary> The name is not meant for UI purposes, but for debugging. </summary>
        public readonly string name = name;
        public readonly List<ParentTask> tasks = tasks;
    }
    
    /// <summary>
    /// Creates a new list of tasks, representing starting the cooking process from scratch.
    /// </summary>
    public static List<TaskGroup> CreateTaskList() =>
    [
        new (name: "Tutorial", tasks: [
            new Group0_MouseLook(),
            new Group0_Move(),
            new Group0_Interact(),
        ]),
        new (name: "Initial prep", tasks: [
            new Group1_WashHands(),
            new Group1_PutOnApron(),
            new Group1_OpenRecipe(),
        ]),
        new (name: "Gather ingredients", tasks: [
            new Group2_GatherIngredients(),
        ])
    ];
    
    #region Group 0
    private class Group0_MouseLook : ParentTask
    {
        public override string description => "Use the mouse to look around";

        public override void SetState_TaskStart()
        {
            PlayerController.instance.OnPlayerLook += Complete;
        }

        public override void SetState_TaskComplete()
        {
            PlayerController.instance.OnPlayerLook -= Complete;
        }
    }

    private class Group0_Move : ParentTask
    {
        // TODO dynamically set keys based on keybindings
        public override string description => "Use WASD to move";
        
        public override void SetState_TaskStart()
        {
            PlayerController.instance.OnPlayerMove += Complete;
        }

        public override void SetState_TaskComplete()
        {
            PlayerController.instance.OnPlayerMove -= Complete;
        }
    }

    private class Group0_Interact : ParentTask
    {
        public override string description => "Use the left mouse button to interact";
        // hint = "Turn on the lights by using the light switch by the door"

        public override void SetState_TaskStart()
        {
            hints.Add("Turn on the lights by using the light switch by the door");
            Game.Interactables[Game.ItemId.LIGHT_SWITCH]
                .AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var lights = Game.Interactables[Game.ItemId.LIGHT_SWITCH] as KitchenLightController;
            lights.RemoveClickListener(Complete);
            lights.SetLightOn(true);
        }
    }
    #endregion
    #region Group 1
    private class Group1_PutOnApron : ParentTask
    {
        public override string description => "Put on apron";
        public override void SetState_TaskStart()
        {
            hints.Add("(It's on the pantry door)");
            Game.Interactables[Game.ItemId.APRON].AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var apron = Game.Interactables[Game.ItemId.APRON] as Apron;
            apron.RemoveClickListener(Complete);
            apron.SetApronVisible(false);
        }
    }

    public class Group1_WashHands : ParentTask
    {
        public override string description => "Wash hands";
        public override void SetState_TaskStart()
        {
            hints.Add("Use sink");
            Game.Sink.AddClickListener(OnSinkClicked);
        }

        private void OnSinkClicked()
        {
            GD.Print("OnSinkClicked()");
            Game.Sink.RemoveClickListener(OnSinkClicked);
            Game.Soap.AddClickListener(OnSoapClicked);
            hints.Add("Use soap");
            Game.UpdateUi();
        }

        private void OnSoapClicked()
        {
            Game.Soap.RemoveClickListener(OnSoapClicked);
            Game.Towel.AddClickListener(Complete);
            hints.Add("Dry hands on towel");
            Game.UpdateUi();
        }

        public override void SetState_TaskComplete()
        {
            Game.Sink.RemoveClickListener(OnSinkClicked);
            Game.Soap.RemoveClickListener(OnSoapClicked);
            Game.Towel.RemoveClickListener(Complete);
        }
    }

    public class Group1_OpenRecipe : ParentTask
    {
        public override string description => "Open recipe";

        public override void SetState_TaskStart()
        {
            hints.Add("Click on the tablet");
            Game.Interactables[Game.ItemId.TABLET].AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var tablet = Game.Interactables[Game.ItemId.TABLET] as Tablet;
            tablet.RemoveClickListener(Complete);
        }
    }
    #endregion
    #region Group 2
    public class Group2_GatherIngredients : ParentTask
    {
        public override string description => "Gather ingredients";
        public override void SetState_TaskStart()
        {
            subtasks.Add(new Group2_Sub_Butter());
            subtasks.Add(new Group2_Sub_Eggs());
            subtasks.Add(new Group2_Sub_Flour());
            subtasks.Add(new Group2_Sub_Sugar());
            subtasks.Add(new Group2_Sub_BrownSugar());
            subtasks.Add(new Group2_Sub_BakingSoda());
            subtasks.Add(new Group2_Sub_Vanilla());
            subtasks.Add(new Group2_Sub_ChocChips());
        }
    }
    public class Group2_Sub_Butter : Subtask
    {
        public override string description => "Butter - 250g";
        public override void SetState_TaskStart()
        {
            var fridge = Game.Interactables[Game.ItemId.FRIDGE] as Fridge;
            if (fridge.isOpen)
                fridge.AddClickListener(OnFridgeClicked, highlight: false);
            else
                fridge.AddClickListener(OnFridgeClicked, highlight: true);
            Game.Interactables[Game.ItemId.BUTTER].AddClickListener(OnButterPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Game.Interactables[Game.ItemId.FRIDGE] as Fridge;
            GD.PrintRaw($"Butter.OnFridgeClicked() - fridge open ? {fridge.isOpen}");
            fridge.RemoveClickListener(OnFridgeClicked);
            if (fridge.isOpen)
                fridge.AddClickListener(OnFridgeClicked, highlight: false);
            else
                fridge.AddClickListener(OnFridgeClicked, highlight: true);
        }

        private void OnButterPickedUp()
        {
            if (!Player.canHoldItem) return;
            
            Game.Interactables[Game.ItemId.FRIDGE].RemoveClickListener(OnFridgeClicked);
            var butter = Game.Interactables[Game.ItemId.BUTTER];
            butter.RemoveClickListener(OnButterPickedUp);
            butter.PickUp();
            Game.Interactables[Game.ItemId.BENCH_PREP_AREA].AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.Interactables[Game.ItemId.FRIDGE].RemoveClickListener(OnFridgeClicked);
            var benchPrepArea = Game.Interactables[Game.ItemId.BENCH_PREP_AREA] as BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var butter = Game.Interactables[Game.ItemId.BUTTER];
            butter.RemoveClickListener(OnButterPickedUp);
            butter.SetPlacement(benchPrepArea.butterPlacement);
        }
    }

    public class Group2_Sub_Eggs : Subtask
    {
        public override string description => "Eggs - x2";
        public override void SetState_TaskStart()
        {
            var fridge = Game.Interactables[Game.ItemId.FRIDGE] as Fridge;
            if (fridge.isOpen)
                fridge.AddClickListener(OnFridgeClicked, highlight: false);
            else
                fridge.AddClickListener(OnFridgeClicked, highlight: true);
            Game.Interactables[Game.ItemId.EGG_CARTON].AddClickListener(OnCartonPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Game.Interactables[Game.ItemId.FRIDGE] as Fridge;
            GD.PrintRaw($"Eggs.OnFridgeClicked() - fridge open ? {fridge.isOpen}");
            fridge.RemoveClickListener(OnFridgeClicked);
            if (fridge.isOpen)
                fridge.AddClickListener(OnFridgeClicked, highlight: false);
            else
                fridge.AddClickListener(OnFridgeClicked, highlight: true);
        }

        private void OnCartonPickedUp()
        {
            if (!Player.canHoldItem) return;
            
            Game.Interactables[Game.ItemId.FRIDGE].RemoveClickListener(OnFridgeClicked);
            var eggCarton = Game.Interactables[Game.ItemId.EGG_CARTON];
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            Player.HoldItem(eggCarton);
            Game.Interactables[Game.ItemId.BENCH_PREP_AREA].AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.Interactables[Game.ItemId.FRIDGE].RemoveClickListener(OnFridgeClicked);
            var benchPrepArea = Game.Interactables[Game.ItemId.BENCH_PREP_AREA] as BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var eggCarton = Game.Interactables[Game.ItemId.EGG_CARTON];
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            eggCarton.SetPlacement(benchPrepArea.eggCartonPlacement);
        }
    }

    public class Group2_Sub_Flour : Subtask
    {
        public override string description => "Flour - 2.5 cups";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.Interactables[Game.ItemId.CUPBOARD_UPPER_DOOR] as Door;
            GD.Print($"Starting flour task - cupboard door open ? {cupboard.isOpen}");
            if (cupboard.isOpen)
                cupboard.AddClickListener(OnCupboardClicked, highlight: false);
            else
                cupboard.AddClickListener(OnCupboardClicked, highlight: true);
            Game.Interactables[Game.ItemId.FLOUR_JAR].AddClickListener(OnFlourClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.Interactables[Game.ItemId.CUPBOARD_UPPER_DOOR] as Door;
            cupboard.RemoveClickListener(OnCupboardClicked);
            if (cupboard.isOpen)
                cupboard.AddClickListener(OnCupboardClicked, highlight: false);
            else
                cupboard.AddClickListener(OnCupboardClicked, highlight: true);
        }

        private void OnFlourClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.Interactables[Game.ItemId.CUPBOARD_UPPER_DOOR].RemoveClickListener(OnCupboardClicked);
            var flour = Game.Interactables[Game.ItemId.FLOUR_JAR];
            flour.RemoveClickListener(OnFlourClicked);
            Player.HoldItem(flour);
            Game.Interactables[Game.ItemId.BENCH_PREP_AREA].AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.Interactables[Game.ItemId.FRIDGE].RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.Interactables[Game.ItemId.BENCH_PREP_AREA] as BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var flour = Game.Interactables[Game.ItemId.FLOUR_JAR];
            flour.RemoveClickListener(OnFlourClicked);
            flour.SetPlacement(benchPrepArea.flourPlacement);
        }
    }

    public class Group2_Sub_Sugar : Subtask
    {
        public override string description => "Sugar - 1 cup";
        public override void SetState_TaskStart()
        {
        }
    }

    public class Group2_Sub_BrownSugar : Subtask
    {
        public override string description => "Brown sugar - 1/2 cup";
        public override void SetState_TaskStart()
        {
        }
    }

    public class Group2_Sub_BakingSoda : Subtask
    {
        public override string description => "Bicarb soda - 1 tsp";
        public override void SetState_TaskStart()
        {
        }
    }

    public class Group2_Sub_Vanilla : Subtask
    {
        public override string description => "Vanilla extract - 1 tsp";
        public override void SetState_TaskStart()
        {
        }
    }

    public class Group2_Sub_ChocChips : Subtask
    {
        public override string description => "Chocolate chips - 2 cups";
        public override void SetState_TaskStart()
        {
        }
    }
    #endregion
}