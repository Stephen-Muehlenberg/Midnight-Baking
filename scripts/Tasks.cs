using System.Collections.Generic;
using Godot;

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
        ]),
        new (name: "Preheat oven", tasks: [
            new Group3_PreheatOven(),
        ]),
        new (name: "Sift dry ingredients", tasks: [
            new Group4_SiftDryIngredients(),
        ]),
    ];
    
    #region Group 0 Tutorial
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

        public override void SetState_TaskStart()
        {
            hints.Add("Turn on the lights by using the light switch by the door");
            Item.LightSwitch.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var lights = Item.LightSwitch;
            lights.RemoveClickListener(Complete);
            lights.SetLightOn(true);
        }
    }
    #endregion
    #region Group 1 Initial prep
    private class Group1_PutOnApron : ParentTask
    {
        public override string description => "Put on apron";
        public override void SetState_TaskStart()
        {
            hints.Add("(It's on the pantry door)");
            Item.Apron.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var apron = Item.Apron;
            apron.RemoveClickListener(Complete);
            apron.SetApronVisible(false);
        }
    }

    private class Group1_WashHands : ParentTask
    {
        public override string description => "Wash hands";
        public override void SetState_TaskStart()
        {
            hints.Add("Use sink");
            Item.Sink.AddClickListener(OnSinkClicked);
        }

        private void OnSinkClicked()
        {
            GD.Print("OnSinkClicked()");
            Item.Sink.RemoveClickListener(OnSinkClicked);
            Item.Soap.AddClickListener(OnSoapClicked);
            hints.Add("Use soap");
            Game.UpdateUi();
        }

        private void OnSoapClicked()
        {
            Item.Soap.RemoveClickListener(OnSoapClicked);
            Item.Towel.AddClickListener(Complete);
            hints.Add("Dry hands on towel");
            Game.UpdateUi();
        }

        public override void SetState_TaskComplete()
        {
            Item.Sink.RemoveClickListener(OnSinkClicked);
            Item.Soap.RemoveClickListener(OnSoapClicked);
            Item.Towel.RemoveClickListener(Complete);
        }
    }

    private class Group1_OpenRecipe : ParentTask
    {
        public override string description => "Open recipe";

        public override void SetState_TaskStart()
        {
            hints.Add("Click on the tablet");
            Item.Tablet.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var tablet = Item.Tablet;
            tablet.RemoveClickListener(Complete);
            tablet.SetTabletOn(true);
        }
    }
    #endregion
    #region Group 2 Gather Ingredients
    private class Group2_GatherIngredients : ParentTask
    {
        public override string description => "Place ingredients on counter";
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
    private class Group2_Sub_Butter : Subtask
    {
        public override string description => "Butter";
        public override void SetState_TaskStart()
        {
            var fridge = Item.Fridge;
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
            Item.Butter.AddClickListener(OnButterPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Item.Fridge;
            fridge.RemoveClickListener(OnFridgeClicked);
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
        }

        private void OnButterPickedUp()
        {
            if (!Player.CanHoldItem(Interactable.Requirement.ONE_HAND))
            if (!Player.canHoldItem) return;
            
            Item.Fridge.RemoveClickListener(OnFridgeClicked);
            var butter = Item.Butter;
            butter.RemoveClickListener(OnButterPickedUp);
            Player.HoldItem(butter);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.Fridge.RemoveClickListener(OnFridgeClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var butter = Item.Butter;
            butter.RemoveClickListener(OnButterPickedUp);
            butter.SetPlacement(benchPrepArea.butterPlacement);
        }
    }

    private class Group2_Sub_Eggs : Subtask
    {
        public override string description => "Eggs";
        public override void SetState_TaskStart()
        {
            var fridge = Item.Fridge;
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
            Item.EggCarton.AddClickListener(OnCartonPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Item.Fridge;
            fridge.RemoveClickListener(OnFridgeClicked);
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
        }

        private void OnCartonPickedUp()
        {
            if (!Player.canHoldItem) return;
            
            Item.Fridge.RemoveClickListener(OnFridgeClicked);
            var eggCarton = Item.EggCarton;
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            Player.HoldItem(eggCarton);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.Fridge.RemoveClickListener(OnFridgeClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var eggCarton = Item.EggCarton;
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            eggCarton.SetPlacement(benchPrepArea.eggCartonPlacement);
        }
    }

    private class Group2_Sub_Flour : Subtask
    {
        public override string description => "Flour";
        public override void SetState_TaskStart()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Item.FlourJar.AddClickListener(OnFlourClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnFlourClicked()
        {
            if (!Player.canHoldItem) return;
            
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var flour = Item.FlourJar;
            flour.RemoveClickListener(OnFlourClicked);
            Player.HoldItem(flour);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var flour = Item.FlourJar;
            flour.RemoveClickListener(OnFlourClicked);
            flour.SetPlacement(benchPrepArea.flourPlacement);
        }
    }

    private class Group2_Sub_Sugar : Subtask
    {
        public override string description => "Sugar";
        public override void SetState_TaskStart()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Item.SugarJar.AddClickListener(OnSugarClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnSugarClicked()
        {
            if (!Player.canHoldItem) return;
            
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Item.SugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            Player.HoldItem(sugar);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var sugar = Item.SugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            sugar.SetPlacement(benchPrepArea.sugarPlacement);
        }
    }

    private class Group2_Sub_BrownSugar : Subtask
    {
        public override string description => "Brown sugar";
        public override void SetState_TaskStart()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Item.BrownSugarJar.AddClickListener(OnSugarClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnSugarClicked()
        {
            if (!Player.canHoldItem) return;
            
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Item.BrownSugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            Player.HoldItem(sugar);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var sugar = Item.BrownSugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            sugar.SetPlacement(benchPrepArea.brownSugarPlacement);
        }
    }

    private class Group2_Sub_BakingSoda : Subtask
    {
        public override string description => "Bicarb soda";
        public override void SetState_TaskStart()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Item.BakingSoda.AddClickListener(OnSodaClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnSodaClicked()
        {
            if (!Player.canHoldItem) return;
            
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var soda = Item.BakingSoda;
            soda.RemoveClickListener(OnSodaClicked);
            Player.HoldItem(soda);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var soda = Item.BakingSoda;
            soda.RemoveClickListener(OnSodaClicked);
            soda.SetPlacement(benchPrepArea.bakingSodaPlacement);
        }
    }

    private class Group2_Sub_Vanilla : Subtask
    {
        public override string description => "Vanilla extract";
        public override void SetState_TaskStart()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Item.VanillaJar.AddClickListener(OnVanillaClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnVanillaClicked()
        {
            if (!Player.canHoldItem) return;
            
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var vanilla = Item.VanillaJar;
            vanilla.RemoveClickListener(OnVanillaClicked);
            Player.HoldItem(vanilla);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var vanilla = Item.VanillaJar;
            vanilla.RemoveClickListener(OnVanillaClicked);
            vanilla.SetPlacement(benchPrepArea.vanillaPlacement);
        }
    }

    private class Group2_Sub_ChocChips : Subtask
    {
        public override string description => "Dark chocolate chips";
        public override void SetState_TaskStart()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Item.ChocChipBag.AddClickListener(OnChocClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Item.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnChocClicked()
        {
            if (!Player.canHoldItem) return;
            
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var chocChips = Item.ChocChipBag;
            chocChips.RemoveClickListener(OnChocClicked);
            Player.HoldItem(chocChips);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            GD.Print("ChocChips.SetTaskComplete()");
            Item.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Item.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var chocChips = Item.ChocChipBag;
            chocChips.RemoveClickListener(OnChocClicked);
            chocChips.SetPlacement(benchPrepArea.chocChipsPlacement);
        }
    }
    #endregion
    #region Group 3 Preheat Oven
    private class Group3_PreheatOven : ParentTask
    {
        // TODO allow user to set metrics vs imperial in the menu
        public override string description => "Preheat oven to 170°c";
        public override void SetState_TaskStart()
        {
            Item.Oven.TemperatureDial.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var oven = Item.Oven;
            oven.TemperatureDial.RemoveClickListener(Complete);
            oven.SetOn(true);
        }
    }
    #endregion
    #region Group 4 Sift Dry Ingredients
    private class Group4_SiftDryIngredients : ParentTask
    {
        // TODO allow user to set metrics vs imperial in the menu
        public override string description => "Sift flour and bicarb soda into a medium bowl";
        public override void SetState_TaskStart()
        {
            subtasks.Add(new Group4_SubCups());
            var bowlTask = new Group4_SubBowl();
            bowlTask.onCompleteCallback += OnBowlPlaced;
            subtasks.Add(bowlTask);
        }
        
        private void OnBowlPlaced()
        {
            AddSubtask(new Group4_SubSieve(), startSubtask: true);
        }

        public override void Complete(bool invokeOnCompleteCallback = true)
        {
        }
    }
    private class Group4_SubCups : Subtask
    {
        public override string description => "Get measuring cups (1 cup and 1/2 cup)";
        private int cupsPickedUp;
        private int cupsPlaced;
        
        public override void SetState_TaskStart()
        {
            cupsPickedUp = 0;
            cupsPlaced = 0;
            var drawer = Item.DrawerUtensils;
            drawer.AddClickListener(OnDrawerClicked, highlight: !drawer.IsOpen);
            Item.MeasuringCupOne.AddClickListener(OnCupOnePickedUp);
            Item.MeasuringCupHalf.AddClickListener(OnCupHalfPickedUp);
        }

        private void OnDrawerClicked()
        {
            var drawer = Item.DrawerUtensils;
            drawer.RemoveClickListener(OnDrawerClicked);
            drawer.AddClickListener(OnDrawerClicked, highlight: !drawer.IsOpen && cupsPickedUp < 2);
        }

        private void OnCupOnePickedUp()
        {
            if (!Player.canHoldItem) return;

            cupsPickedUp++;
            var cup = Item.MeasuringCupOne;
            cup.RemoveClickListener(OnCupOnePickedUp);
            Player.HoldItem(cup);
            Item.BenchPrepArea.AddClickListener(OnBenchClicked);
        }

        private void OnCupHalfPickedUp()
        {
            if (!Player.canHoldItem) return;

            cupsPickedUp++;
            var cup = Item.MeasuringCupHalf;
            cup.RemoveClickListener(OnCupHalfPickedUp);
            Player.HoldItem(cup);
            Item.BenchPrepArea.AddClickListener(OnBenchClicked);
        }

        private void OnBenchClicked()
        {
            cupsPlaced++;
            if (cupsPlaced == 2)
            {
                Complete();
                return;
            }
            var cup = Player.heldItem;
            var bench = Item.BenchPrepArea;
            bench.RemoveClickListener(OnBenchClicked);
            var newPosition = cup.idTest == Item.Id.Measuring_FullCup
                ? bench.measuringCupOnePlacement
                : bench.measuringCupHalfPlacement;
            cup.SetPlacement(newPosition);
        }

        public override void SetState_TaskComplete()
        {
            var cupOne = Item.MeasuringCupOne;
            var cupHalf = Item.MeasuringCupHalf;
            var bench = Item.BenchPrepArea;
            Item.DrawerUtensils.RemoveClickListener(OnDrawerClicked);
            bench.RemoveClickListener(OnBenchClicked);
            cupOne.RemoveClickListener(OnCupOnePickedUp);
            cupHalf.RemoveClickListener(OnCupHalfPickedUp);
            cupOne.SetPlacement(bench.measuringCupOnePlacement);
            cupHalf.SetPlacement(bench.measuringCupHalfPlacement);
        }
    }
    private class Group4_SubBowl : Subtask
    {
        public override string description => "Get medium bowl";

        public override void SetState_TaskStart()
        {
            Item.MixingBowlMedium.AddClickListener(OnBowlClicked);
        }

        private void OnBowlClicked()
        {
            var bowl = Item.MixingBowlMedium;
            bowl.RemoveClickListener(OnBowlClicked);
            Player.HoldItem(bowl);
            Item.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var bowl = Item.MixingBowlMedium;
            var bench = Item.BenchPrepArea;
            bowl.RemoveClickListener(OnBowlClicked);
            bench.RemoveClickListener(Complete);
            bowl.SetPlacement(bench.mixingBowlPlacement);
        }
    }
    private class Group4_SubSieve : Subtask
    {
        public override string description => "Get sieve";

        public override void SetState_TaskStart()
        {
            Item.Sieve.AddClickListener(OnSieveClicked);
            GD.Print("SubSieve.SetState_TaskStart() - ");
        }

        private void OnSieveClicked()
        {
            GD.Print("OnSieveClicked");
            if (!Player.canHoldItem) return;
            
            var sieve = Item.Sieve;
            sieve.RemoveClickListener(OnSieveClicked);
            Player.HoldItem(sieve);
            Item.MixingBowlMedium.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var sieve = Item.Sieve;
            var bowl = Item.MixingBowlMedium;
            sieve.RemoveClickListener(OnSieveClicked);
            bowl.RemoveClickListener(Complete);
            sieve.SetPlacement(bowl.sievePivot);
        }
    }
    #endregion
}