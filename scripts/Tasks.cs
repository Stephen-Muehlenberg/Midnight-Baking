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
            Game.LightSwitch.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var lights = Game.LightSwitch;
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
            Game.Apron.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var apron = Game.Apron;
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

    private class Group1_OpenRecipe : ParentTask
    {
        public override string description => "Open recipe";

        public override void SetState_TaskStart()
        {
            hints.Add("Click on the tablet");
            Game.Tablet.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var tablet = Game.Tablet;
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
            var fridge = Game.Fridge;
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
            Game.Butter.AddClickListener(OnButterPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Game.Fridge;
            fridge.RemoveClickListener(OnFridgeClicked);
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
        }

        private void OnButterPickedUp()
        {
            if (!Player.canHoldItem) return;
            
            Game.Fridge.RemoveClickListener(OnFridgeClicked);
            var butter = Game.Butter;
            butter.RemoveClickListener(OnButterPickedUp);
            Player.HoldItem(butter);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.Fridge.RemoveClickListener(OnFridgeClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var butter = Game.Butter;
            butter.RemoveClickListener(OnButterPickedUp);
            butter.SetPlacement(benchPrepArea.butterPlacement);
        }
    }

    private class Group2_Sub_Eggs : Subtask
    {
        public override string description => "Eggs";
        public override void SetState_TaskStart()
        {
            var fridge = Game.Fridge;
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
            Game.EggCarton.AddClickListener(OnCartonPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Game.Fridge;
            fridge.RemoveClickListener(OnFridgeClicked);
            fridge.AddClickListener(OnFridgeClicked, highlight: !fridge.isOpen);
        }

        private void OnCartonPickedUp()
        {
            if (!Player.canHoldItem) return;
            
            Game.Fridge.RemoveClickListener(OnFridgeClicked);
            var eggCarton = Game.EggCarton;
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            Player.HoldItem(eggCarton);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.Fridge.RemoveClickListener(OnFridgeClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var eggCarton = Game.EggCarton;
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            eggCarton.SetPlacement(benchPrepArea.eggCartonPlacement);
        }
    }

    private class Group2_Sub_Flour : Subtask
    {
        public override string description => "Flour";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Game.FlourJar.AddClickListener(OnFlourClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnFlourClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var flour = Game.FlourJar;
            flour.RemoveClickListener(OnFlourClicked);
            Player.HoldItem(flour);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var flour = Game.FlourJar;
            flour.RemoveClickListener(OnFlourClicked);
            flour.SetPlacement(benchPrepArea.flourPlacement);
        }
    }

    private class Group2_Sub_Sugar : Subtask
    {
        public override string description => "Sugar";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Game.SugarJar.AddClickListener(OnSugarClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnSugarClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Game.SugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            Player.HoldItem(sugar);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var sugar = Game.SugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            sugar.SetPlacement(benchPrepArea.sugarPlacement);
        }
    }

    private class Group2_Sub_BrownSugar : Subtask
    {
        public override string description => "Brown sugar";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Game.BrownSugarJar.AddClickListener(OnSugarClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnSugarClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Game.BrownSugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            Player.HoldItem(sugar);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var sugar = Game.BrownSugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            sugar.SetPlacement(benchPrepArea.brownSugarPlacement);
        }
    }

    private class Group2_Sub_BakingSoda : Subtask
    {
        public override string description => "Bicarb soda";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Game.BakingSoda.AddClickListener(OnSodaClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnSodaClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var soda = Game.BakingSoda;
            soda.RemoveClickListener(OnSodaClicked);
            Player.HoldItem(soda);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var soda = Game.BakingSoda;
            soda.RemoveClickListener(OnSodaClicked);
            soda.SetPlacement(benchPrepArea.bakingSodaPlacement);
        }
    }

    private class Group2_Sub_Vanilla : Subtask
    {
        public override string description => "Vanilla extract";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Game.VanillaJar.AddClickListener(OnVanillaClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnVanillaClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var vanilla = Game.VanillaJar;
            vanilla.RemoveClickListener(OnVanillaClicked);
            Player.HoldItem(vanilla);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var vanilla = Game.VanillaJar;
            vanilla.RemoveClickListener(OnVanillaClicked);
            vanilla.SetPlacement(benchPrepArea.vanillaPlacement);
        }
    }

    private class Group2_Sub_ChocChips : Subtask
    {
        public override string description => "Dark chocolate chips";
        public override void SetState_TaskStart()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
            Game.ChocChipBag.AddClickListener(OnChocClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Game.CupboardUpper;
            cupboard.RemoveClickListener(OnCupboardClicked);
            cupboard.AddClickListener(OnCupboardClicked, highlight: !cupboard.isOpen);
        }

        private void OnChocClicked()
        {
            if (!Player.canHoldItem) return;
            
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var chocChips = Game.ChocChipBag;
            chocChips.RemoveClickListener(OnChocClicked);
            Player.HoldItem(chocChips);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            GD.Print("ChocChips.SetTaskComplete()");
            Game.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var benchPrepArea = Game.BenchPrepArea;
            benchPrepArea.RemoveClickListener(Complete);
            var chocChips = Game.ChocChipBag;
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
            Game.Oven.TemperatureDial.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var oven = Game.Oven;
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
            subtasks.Add(new Group4_SubBowl());
//            hints.Add("Get bowl, sieve, measuring cups, and measuring spoons on bench");
  //          Game.MixingBowlMedium.AddClickListener(OnBowlPickedUp);
        }

        private void OnBowlPickedUp()
        {
            var bowl = Game.MixingBowlMedium;
            bowl.RemoveClickListener(OnBowlPickedUp);
            Player.HoldItem(bowl);
            Game.BenchPrepArea.AddClickListener(OnBowlPlaced);
        }

        private void OnBowlPlaced()
        {
            var bench = Game.BenchPrepArea;
            bench.RemoveClickListener(OnBowlPlaced);
            Game.MixingBowlMedium.SetPlacement(bench.mixingBowlPlacement);
            hints.Add("Place sieve in bowl");
            Game.Sieve.AddClickListener(OnSievePickedUp);
        }

        private void OnSievePickedUp()
        {
            var sieve = Game.Sieve;
            sieve.RemoveClickListener(OnSievePickedUp);
            Player.HoldItem(sieve);
            Game.MixingBowlMedium.AddClickListener(OnSievePlaced);
        }

        private void OnSievePlaced()
        {
            var bowl = Game.MixingBowlMedium;
            var sieve =  Game.Sieve;
            sieve.SetPlacement(bowl);
            hints.Add("Get measuring cups and spoons");
        }

        public override void Complete(bool invokeOnCompleteCallback = true)
        {
            var bench = Game.BenchPrepArea;
            bench.RemoveClickListener(OnBowlPlaced);
            var bowl = Game.MixingBowlMedium;
            bowl.RemoveClickListener(OnBowlPickedUp);
            var sieve = Game.Sieve;
            sieve.RemoveClickListener(OnSievePickedUp);
            Game.MixingBowlMedium.SetPlacement(bench.mixingBowlPlacement);
            
        }
    }
    private class Group4_SubCups : Subtask
    {
        public override string description => "Get measuring cups (1 cup and 1/2 cup)";
        private int cupsPlaced;
        
        public override void SetState_TaskStart()
        {
            cupsPlaced = 0;
            var drawer = Game.DrawerUtensils;
            drawer.AddClickListener(OnDrawerClicked, highlight: !drawer.IsOpen);
            Game.MeasuringCupOne.AddClickListener(OnCupOnePickedUp);
            Game.MeasuringCupHalf.AddClickListener(OnCupHalfPickedUp);
        }

        private void OnDrawerClicked()
        {
            var drawer = Game.DrawerUtensils;
            drawer.RemoveClickListener(OnDrawerClicked);
            drawer.AddClickListener(OnDrawerClicked, highlight: !drawer.IsOpen);
        }

        private void OnCupOnePickedUp()
        {
            if (!Player.canHoldItem) return;

            var cup = Game.MeasuringCupOne;
            cup.RemoveClickListener(OnCupOnePickedUp);
            Player.HoldItem(cup);
            Game.BenchPrepArea.AddClickListener(OnBenchClicked);
        }

        private void OnCupHalfPickedUp()
        {
            if (!Player.canHoldItem) return;

            var cup = Game.MeasuringCupHalf;
            cup.RemoveClickListener(OnCupHalfPickedUp);
            Player.HoldItem(cup);
            Game.BenchPrepArea.AddClickListener(OnBenchClicked);
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
            var bench = Game.BenchPrepArea;
            bench.RemoveClickListener(OnBenchClicked);
            var newPosition = cup.itemId == Game.ItemId.MEASURING_CUP_ONE
                ? bench.measuringCupOnePlacement
                : bench.measuringCupHalfPlacement;
            cup.SetPlacement(newPosition);
        }

        public override void SetState_TaskComplete()
        {
            var cupOne = Game.MeasuringCupOne;
            var cupHalf = Game.MeasuringCupHalf;
            var bench = Game.BenchPrepArea;
            Game.DrawerUtensils.RemoveClickListener(OnDrawerClicked);
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
            Game.MixingBowlMedium.AddClickListener(OnBowlClicked);
        }

        private void OnBowlClicked()
        {
            var bowl = Game.MixingBowlMedium;
            bowl.RemoveClickListener(OnBowlClicked);
            Player.HoldItem(bowl);
            Game.BenchPrepArea.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var bowl = Game.MixingBowlMedium;
            var bench = Game.BenchPrepArea;
            bowl.RemoveClickListener(OnBowlClicked);
            bench.RemoveClickListener(Complete);
            bowl.SetPlacement(bench.mixingBowlPlacement);
        }
    }
    #endregion
}