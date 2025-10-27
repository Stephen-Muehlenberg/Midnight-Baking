using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public static class Tasks
{
    /// <summary>
    /// Creates a new list of tasks, representing starting the cooking process from scratch.
    /// </summary>
    public static List<Game.TaskGroup> CreateTaskList() =>
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
    private class Group0_MouseLook : Task
    {
        public override string description => "Use the mouse to look around";

        protected override void SetState_TaskStart()
        {
            PlayerController.FetchInstance().OnPlayerLook += Complete;
        }

        protected override void SetState_TaskComplete()
        {
            PlayerController.FetchInstance().OnPlayerLook -= Complete;
        }
    }

    private class Group0_Move : Task
    {
        // TODO dynamically set keys based on keybindings
        public override string description => "Use WASD to move";
        
        protected override void SetState_TaskStart()
        {
            PlayerController.FetchInstance().OnPlayerMove += Complete;
        }

        protected override void SetState_TaskComplete()
        {
            PlayerController.FetchInstance().OnPlayerMove -= Complete;
        }
    }

    private class Group0_Interact : Task
    {
        public override string description => "Use the left mouse button to interact";
        // hint = "Turn on the lights by using the light switch by the door"

        protected override void SetState_TaskStart()
        {
            hints.Add("Turn on the lights by using the light switch by the door");
            Game.Interactables[Game.ItemId.LIGHT_SWITCH]
                .SetHighlighted(onClickCallback: Complete);
        }

        protected override void SetState_TaskComplete()
        {
            var lights = Game.Interactables[Game.ItemId.LIGHT_SWITCH] as KitchenLightController;
            lights.SetLightOn(true);
            lights.SetCanInteract(true);
        }
    }
    #endregion
    #region Group 1
    private class Group1_PutOnApron : Task
    {
        public override string description => "Put on apron";
        protected override void SetState_TaskStart()
        {
            hints.Add("(It's on the pantry door)");
            Game.Interactables[Game.ItemId.APRON].SetHighlighted(onClickCallback: Complete);
        }

        protected override void SetState_TaskComplete()
        {
            var apron = Game.Interactables[Game.ItemId.APRON] as Apron;
            apron.SetCanInteract(false);
            apron.SetApronVisible(false);
        }
    }

    public class Group1_WashHands : Task
    {
        public override string description => "Wash hands";
        protected override void SetState_TaskStart()
        {
            hints.Add("Use sink");
            Game.Interactables[Game.ItemId.SINK].SetHighlighted(onClickCallback: OnSinkClicked);
        }

        private void OnSinkClicked()
        {
            Game.Interactables[Game.ItemId.SINK].SetCanInteract(true);
            hints.Add("Dry hands on towel");
            Game.UpdateUi();
            Game.Interactables[Game.ItemId.TOWEL].SetHighlighted(onClickCallback: Complete);
        }

        protected override void SetState_TaskComplete()
        {
            Game.Interactables[Game.ItemId.SINK].SetCanInteract(true);
            Game.Interactables[Game.ItemId.TOWEL].SetCanInteract(false);
        }
    }

    public class Group1_OpenRecipe : Task
    {
        public override string description => "Open recipe";

        protected override void SetState_TaskStart()
        {
            hints.Add("Click on the tablet");
            Game.Interactables[Game.ItemId.TABLET].SetHighlighted(onClickCallback: Complete);
        }

        protected override void SetState_TaskComplete()
        {
            var tablet = Game.Interactables[Game.ItemId.TABLET] as Tablet;
            tablet.SetTabletOn(true);
            tablet.SetCanInteract(true);
        }
    }
    #endregion
    #region Group 2
    public class Group2_GatherIngredients : Task
    {
        public override string description => "Gather ingredients";
        protected override void SetState_TaskStart()
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
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_Eggs : Subtask
    {
        public override string description => "Eggs - x2";
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_Flour : Subtask
    {
        public override string description => "Flour - 2.5 cups";
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_Sugar : Subtask
    {
        public override string description => "Sugar - 1 cup";
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_BrownSugar : Subtask
    {
        public override string description => "Brown sugar - 1/2 cup";
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_BakingSoda : Subtask
    {
        public override string description => "Bicarb soda - 1 tsp";
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_Vanilla : Subtask
    {
        public override string description => "Vanilla extract - 1 tsp";
        public override void SetState_SubtaskStart()
        {
        }
    }

    public class Group2_Sub_ChocChips : Subtask
    {
        public override string description => "Chocolate chips - 2 cups";
        public override void SetState_SubtaskStart()
        {
        }
    }
    #endregion
}