using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts.tasks;

public static class Tasks
{
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
            new Group1_PickSomeTunes(),
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
        new (name: "Soften butter", tasks: [
            new Group5_SoftenButter(),
        ]),
    ];
    
    #region Group 0 Tutorial
    private class Group0_MouseLook : ParentTask
    {
        public override string description => "Use the mouse to look around";

        protected override void SetState_TaskStart()
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

        protected override void SetState_TaskStart()
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

        protected override void SetState_TaskStart()
        {
            hints.Add("Turn on the lights by using the light switch by the door");
            AddClickListener(Items.LightSwitch, Complete);
        }

        public override void DoTeardown()
        {
            Items.LightSwitch.RemoveClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Items.LightSwitch.SetLightOn(true);
        }
    }
    #endregion
    #region Group 1 Initial prep
    private class Group1_PutOnApron : ParentTask
    {
        public override string description => "Put on apron";

        protected override void SetState_TaskStart()
        {
            hints.Add("(It's on the pantry door)");
            AddClickListener(Items.Apron, Complete);
        }

        public override void DoTeardown()
        {
            Items.Apron.RemoveClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Items.Apron.SetApronVisible(false);
        }
    }

    private class Group1_WashHands : ParentTask
    {
        public override string description => "Wash hands";

        protected override void SetState_TaskStart()
        {
            hints.Add("Use sink");
            AddClickListener(Items.Sink, OnSinkClicked);
        }

        private void OnSinkClicked()
        {
            AddClickListener(Items.Soap, OnSoapClicked);
            hints.Add("Use soap");
            Game.UpdateUi();
        }

        private void OnSoapClicked()
        {
            AddClickListener(Items.Towel, Complete);
            hints.Add("Dry hands on towel");
            Game.UpdateUi();
        }

        public override void DoTeardown()
        {
            Items.Sink.RemoveClickListener(OnSinkClicked);
            Items.Soap.RemoveClickListener(OnSoapClicked);
            Items.Towel.RemoveClickListener(Complete);
        }
    }

    private class Group1_OpenRecipe : ParentTask
    {
        public override string description => "Open recipe";

        protected override void SetState_TaskStart()
        {
            hints.Add("Click on the tablet");
            Items.Tablet.AddClickListener(Complete);
        }

        public override void DoTeardown()
        {
            Items.Tablet.RemoveClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Items.Tablet.SetTabletOn(true);
        }
    }

    private class Group1_PickSomeTunes : ParentTask
    {
        public override string description => "Listen to music";
        private int stationsRemaining;
        private SceneTreeTimer timer;

        protected override void SetState_TaskStart()
        {
            hints.Add("See what's on the radio");
            AddClickListener(Items.Radio, OnRadioClicked, autoUnsubscribe: false);
            stationsRemaining = 5;
            timer = null;
        }

        private void OnRadioClicked()
        {
            stationsRemaining--;

            if (stationsRemaining > 0)
            {
                GD.Print($"Stations remaining {stationsRemaining}");
                string secondHint = $"Check {stationsRemaining} more stations, or stick with this one";
                if (hints.Count == 1)
                    hints.Add(secondHint);
                else hints[1] = secondHint;
                Game.UpdateUi();

                if (timer != null)
                    timer.Timeout -= Complete;
                timer = Game.instance.GetTree().CreateTimer(6, false);
                timer.Timeout += Complete;
            }
            else
            {
                hints[1] = "Check 0 more stations, or stick with this one";
                Game.UpdateUi();
                Complete();
            }
        }

        public override void DoTeardown()
        {
            if (timer != null)
            {
                timer.Timeout -= Complete;
                timer = null;
            }
            Items.Radio.RemoveClickListener(OnRadioClicked);
        }
    }
    #endregion
    #region Group 2 Gather Ingredients
    private class Group2_GatherIngredients : ParentTask
    {
        public override string description => "Place ingredients on counter";

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
    private class Group2_Sub_Butter : Subtask
    {
        public override string description => "Butter";

        protected override void SetState_TaskStart()
        {
            var fridge = Items.Fridge;
            AddClickListener(fridge, OnFridgeClicked, highlight: !fridge.isOpen, Interactable.Requirement.REQUIRES_ONE_HAND);
            AddClickListener(Items.Butter, OnButterClicked);
        }

        private void OnFridgeClicked()
        {
            var fridge = Items.Fridge;
            AddClickListener(fridge, OnFridgeClicked, highlight: !fridge.isOpen, Interactable.Requirement.REQUIRES_ONE_HAND);
        }

        private void OnButterClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND))
                return;
            
            Items.Fridge.RemoveClickListener(OnFridgeClicked);
            var butter = Items.Butter;
            Player.PickUpItem(butter);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(butter, butter.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }
        
        public override void DoTeardown()
        {
            Items.Fridge.RemoveClickListener(OnFridgeClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.Butter.RemoveClickListener(OnButterClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.Butter.SetPlacement(Items.BenchPrepArea.butterPlacement);
        }
    }
    private class Group2_Sub_Eggs : Subtask
    {
        public override string description => "Eggs";

        protected override void SetState_TaskStart()
        {
            var fridge = Items.Fridge;
            AddClickListener(fridge, OnFridgeClicked, highlight: () => !fridge.isOpen, Interactable.Requirement.REQUIRES_ONE_HAND);
            AddClickListener(Items.EggCarton, OnCartonPickedUp);
        }

        private void OnFridgeClicked()
        {
            var fridge = Items.Fridge;
            AddClickListener(fridge, OnFridgeClicked, highlight: () => !fridge.isOpen, Interactable.Requirement.REQUIRES_ONE_HAND);
        }

        private void OnCartonPickedUp()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.Fridge.RemoveClickListener(OnFridgeClicked);
            var eggs = Items.EggCarton;
            Player.PickUpItem(eggs);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(eggs, eggs.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }
        
        public override void DoTeardown()
        {
            Items.Fridge.RemoveClickListener(OnFridgeClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.EggCarton.RemoveClickListener(OnCartonPickedUp);
        }

        public override void SetState_TaskComplete()
        {
            Items.EggCarton.SetPlacement(Items.BenchPrepArea.eggCartonPlacement);
        }
    }
    private class Group2_Sub_Flour : Subtask
    {
        public override string description => "Flour";

        protected override void SetState_TaskStart()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
            AddClickListener(Items.FlourJar, OnFlourClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnFlourClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var flour = Items.FlourJar;
            Player.PickUpItem(flour);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(flour, flour.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }

        public override void DoTeardown()
        {
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.FlourJar.RemoveClickListener(OnFlourClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.FlourJar.SetPlacement(Items.BenchPrepArea.flourPlacement);
        }
    }
    private class Group2_Sub_Sugar : Subtask
    {
        public override string description => "Sugar";

        protected override void SetState_TaskStart()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
            AddClickListener(Items.SugarJar, OnSugarClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnSugarClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Items.SugarJar;
            Player.PickUpItem(sugar);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(sugar, sugar.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }
        
        public override void DoTeardown()
        {
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.SugarJar.RemoveClickListener(OnSugarClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.SugarJar.SetPlacement(Items.BenchPrepArea.sugarPlacement);
        }
    }
    private class Group2_Sub_BrownSugar : Subtask
    {
        public override string description => "Brown sugar";

        protected override void SetState_TaskStart()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
            AddClickListener(Items.BrownSugarJar, OnSugarClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);

        }

        private void OnSugarClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Items.BrownSugarJar;
            Player.PickUpItem(sugar);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(sugar, sugar.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }

        public override void DoTeardown()
        {
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.BrownSugarJar.RemoveClickListener(OnSugarClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.BrownSugarJar.SetPlacement(Items.BenchPrepArea.brownSugarPlacement);
        }
    }
    private class Group2_Sub_BakingSoda : Subtask
    {
        public override string description => "Bicarb soda";

        protected override void SetState_TaskStart()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
            AddClickListener(Items.BakingSoda, OnSodaClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnSodaClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var soda = Items.BakingSoda;
            Player.PickUpItem(soda);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(soda, soda.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }

        public override void DoTeardown()
        {
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.BakingSoda.RemoveClickListener(OnSodaClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.BakingSoda.SetPlacement(Items.BenchPrepArea.bakingSodaPlacement);
        }
    }
    private class Group2_Sub_Vanilla : Subtask
    {
        public override string description => "Vanilla extract";

        protected override void SetState_TaskStart()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
            AddClickListener(Items.VanillaJar, OnVanillaClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnVanillaClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var vanilla = Items.VanillaJar;
            Player.PickUpItem(vanilla);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(vanilla, vanilla.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }

        public override void DoTeardown()
        {
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.VanillaJar.RemoveClickListener(OnVanillaClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.VanillaJar.SetPlacement(Items.BenchPrepArea.vanillaPlacement);
        }
    }
    private class Group2_Sub_ChocChips : Subtask
    {
        public override string description => "Dark chocolate chips";

        protected override void SetState_TaskStart()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
            AddClickListener(Items.ChocChipBag, OnChocClicked);
        }

        private void OnCupboardClicked()
        {
            var cupboard = Items.CupboardUpper;
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnChocClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var chocChips = Items.ChocChipBag;
            Player.PickUpItem(chocChips);
            AddClickListener(Items.BenchPrepArea,
                () => Player.PutDownItem(chocChips, chocChips.positionBenchPrepArea, onAnimationEndCallback: Complete));
        }

        public override void DoTeardown()
        {
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
            Items.ChocChipBag.RemoveClickListener(OnChocClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.ChocChipBag.SetPlacement(Items.BenchPrepArea.chocChipsPlacement);
        }
    }
    #endregion
    #region Group 3 Preheat Oven
    private class Group3_PreheatOven : ParentTask
    {
        // TODO allow user to set metrics vs imperial in the menu
        public override string description => "Preheat oven to 170°c";

        protected override void SetState_TaskStart()
        {
            var oven = Items.Oven;
            AddClickListener(oven.TemperatureDial, OnDialClicked);
        }

        private void OnDialClicked()
        {
            Items.Oven.AnimateOn(Complete);
        }

        public override void DoTeardown()
        {
            Items.Oven.TemperatureDial.RemoveClickListener(OnDialClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.Oven.SetOn(true);
        }
    }
    #endregion
    #region Group 4 Sift Dry Ingredients
    private class Group4_SiftDryIngredients : ParentTask
    {
        private bool cupsPlaced, bowlPlaced, sievePlaced, flourAdded, sodaAdded;
        private int cupsAndSievePlacedCount = 0;
        
        // TODO allow user to set metrics vs imperial in the menu
        public override string description => "Sift flour and bicarb soda into a medium bowl";

        public override void Save(Dictionary data)
        {
            data["cups"] = cupsPlaced;
            data["bowl"] = bowlPlaced;
            data["sieve"] = sievePlaced;
            data["flour"] = flourAdded;
            data["soda"] = sodaAdded;
        }

        public override void Load(Dictionary data)
        {
            // TODO handle loading when some subtasks are already complete.
        }

        protected override void SetState_TaskStart()
        {
            var cupsTask = new Group4_SubCups();
            cupsTask.onCompleteCallback += OnCupsOrSievePlaced;
            subtasks.Add(cupsTask);
            
            var bowlTask = new Group4_SubBowl();
            bowlTask.onCompleteCallback += OnBowlPlaced;
            subtasks.Add(bowlTask);
        }
        
        private void OnBowlPlaced()
        {
            var sieveTask = new Group4_SubSieve();
            sieveTask.onCompleteCallback += OnCupsOrSievePlaced;
            AddSubtask(sieveTask, startSubtask: true);
        }

        private void OnCupsOrSievePlaced()
        {
            cupsAndSievePlacedCount++;
            if (cupsAndSievePlacedCount < 2) return;
            
            var flourTask = new Group4_SubAddFlour();
            flourTask.onCompleteCallback += OnFlourAdded;
            AddSubtask(flourTask, startSubtask: true);
        }

        private void OnFlourAdded()
        {
            var sodaTask = new Group4_SubAddBakingSoda();
            sodaTask.onCompleteCallback += OnSodaAdded;
            AddSubtask(sodaTask, startSubtask: true);
        }

        private void OnSodaAdded()
        {
            var siftTask = new Group4_SubSiftIngredients();
            siftTask.onCompleteCallback += Complete; // TODO this isn't actually the end; add next step when ready.
            AddSubtask(siftTask, startSubtask: true);
        }
    }
    private class Group4_SubCups : Subtask
    {
        public override string description => "Get measuring cups (1 cup and 1/2 cup) and spoon (1 teaspoon)";
        private int cupsPickedUp;
        private int measuringToolsPlaced;

        protected override void SetState_TaskStart()
        {
            cupsPickedUp = 0;
            measuringToolsPlaced = 0;
            var drawer = Items.DrawerUtensils;
            AddClickListener(drawer, OnDrawerClicked, highlight: !drawer.IsOpen);
            AddClickListener(Items.MeasuringCupOne, OnCupOneClicked);
            AddClickListener(Items.MeasuringCupHalf, OnCupHalfClicked);
            AddClickListener(Items.MeasuringSpoon, OnSpoonClicked);
        }

        private void OnDrawerClicked()
        {
            var drawer = Items.DrawerUtensils;
            AddClickListener(drawer, OnDrawerClicked, highlight: !drawer.IsOpen && cupsPickedUp < 2);
        }

        private void OnCupOneClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;

            cupsPickedUp++;
            var cup = Items.MeasuringCupOne;
            Player.PickUpItem(cup);
            AddClickListener(Items.BenchPrepArea, OnFullCupPlaced);
        }

        private void OnCupHalfClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;

            cupsPickedUp++;
            var cup = Items.MeasuringCupHalf;
            Player.PickUpItem(cup);
            AddClickListener(Items.BenchPrepArea, OnHalfCupPlaced);
        }

        private void OnSpoonClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;

            cupsPickedUp++;
            var spoon = Items.MeasuringSpoon;
            Player.PickUpItem(spoon);
            AddClickListener(Items.BenchPrepArea, OnSpoonPlaced);
        }

        private void OnFullCupPlaced()
        {
            var bench = Items.BenchPrepArea;
            var cup = Player.IsHoldingInLeftHand(Items.MeasuringCupOne)
                ? Player.heldItemLeft
                : Player.heldItemRight;
            Player.PutDownItem(cup as Pickupable, bench.measuringCupOnePlacement,
                onAnimationEndCallback: () =>
                {
                    measuringToolsPlaced++;
                    if (measuringToolsPlaced == 3)
                        Complete();
                });
        }

        private void OnHalfCupPlaced()
        {
            var bench = Items.BenchPrepArea;
            var cup = Player.IsHoldingInLeftHand(Items.MeasuringCupHalf)
                ? Player.heldItemLeft
                : Player.heldItemRight;
            Player.PutDownItem(cup as Pickupable, bench.measuringCupHalfPlacement,
                onAnimationEndCallback: () =>
                {
                    measuringToolsPlaced++;
                    if (measuringToolsPlaced == 3)
                        Complete();
                });
        }

        private void OnSpoonPlaced()
        {
            var spoon = Items.MeasuringSpoon;
            Player.PutDownItem(spoon, spoon.positionBenchPrepArea,
                onAnimationEndCallback: () =>
                {
                    measuringToolsPlaced++;
                    if (measuringToolsPlaced == 3)
                        Complete();
                });
        }

        public override void DoTeardown()
        {
            Items.DrawerUtensils.RemoveClickListener(OnDrawerClicked);
            Items.BenchPrepArea.RemoveClickListener(OnFullCupPlaced);
            Items.BenchPrepArea.RemoveClickListener(OnHalfCupPlaced);
            Items.BenchPrepArea.RemoveClickListener(OnSpoonPlaced);
            Items.MeasuringCupOne.RemoveClickListener(OnCupOneClicked);
            Items.MeasuringCupHalf.RemoveClickListener(OnCupHalfClicked);
            Items.MeasuringSpoon.RemoveClickListener(OnSpoonClicked);
        }

        public override void SetState_TaskComplete()
        {
            var bench = Items.BenchPrepArea;
            Items.MeasuringCupOne.SetPlacement(bench.measuringCupOnePlacement);
            Items.MeasuringCupHalf.SetPlacement(bench.measuringCupHalfPlacement);
            Items.MeasuringSpoon.SetPlacement(bench.measuringSpoonPlacement);
        }
    }
    private class Group4_SubBowl : Subtask
    {
        public override string description => "Get medium bowl";

        protected override void SetState_TaskStart()
        {
            AddClickListener(Items.MixingBowlMedium, OnBowlClicked);
        }

        private void OnBowlClicked()
        {
            Player.PickUpItem(Items.MixingBowlMedium);
            AddClickListener(Items.BenchPrepArea, OnBenchClicked);
        }

        private void OnBenchClicked()
        {
            var bowl = Items.MixingBowlMedium;
            Player.PutDownItem(bowl, bowl.positionBenchPrepArea, onAnimationEndCallback: Complete);
        }

        public override void DoTeardown()
        {
            Items.MixingBowlMedium.RemoveClickListener(OnBowlClicked);
            Items.BenchPrepArea.RemoveClickListener(OnBenchClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.MixingBowlMedium.SetPlacement(Items.BenchPrepArea.mixingBowlPlacement);
        }
    }
    private class Group4_SubSieve : Subtask
    {
        public override string description => "Get sieve";

        protected override void SetState_TaskStart()
        {
            AddClickListener(Items.Sieve, OnSieveClicked);
        }

        private void OnSieveClicked()
        {
            Player.PickUpItem(Items.Sieve);
            AddClickListener(Items.MixingBowlMedium, OnBowlClicked, requirementOverride: Interactable.Requirement.NO_REQUIREMENT);
        }

        private void OnBowlClicked()
        {
            Player.PutDownItem(Items.Sieve, Items.MixingBowlMedium.sievePivot, onAnimationEndCallback: Complete);
        }

        public override void DoTeardown()
        {
            Items.Sieve.RemoveClickListener(OnSieveClicked);
            Items.MixingBowlMedium.RemoveClickListener(OnBowlClicked);
        }

        public override void SetState_TaskComplete()
        {
            Items.Sieve.SetPlacement(Items.MixingBowlMedium.sievePivot);
        }
    }
    private class Group4_SubAddFlour : Subtask
    {
        private int halfCupsFlourAdded;
        public override string description => "Add 2.5 cups flour";

        protected override void SetState_TaskStart()
        {
            AddClickListener(Items.FlourJar, OnFlourClicked_OpenJar);
        }

        private void OnFlourClicked_OpenJar()
        {
            var flourJar = Items.FlourJar;
            var cupHalf = Items.MeasuringCupHalf;
            var cupOne = Items.MeasuringCupOne;

            flourJar.Lid.SetPlacement(flourJar.Lid.positionBenchPrepArea);
            flourJar.SetLidAttached(false);

            AddClickListener(cupHalf, OnMeasuringCupHalfClicked, 
                highlight: () => {
                    if (Player.IsHolding(cupHalf)) return false;
                    return Player.IsHolding(cupOne) || Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND);
                },
                autoUnsubscribe: false);
            AddClickListener(cupOne, OnMeasuringCupOneClicked,
                highlight: () => {
                    if (Player.IsHolding(cupOne)) return false;
                    return Player.IsHolding(cupHalf) || Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND);
                },
                autoUnsubscribe: false);
            AddClickListener(flourJar, OnFlourClicked_ScoopFlour,
                highlight: () => (Player.IsHolding(cupOne) && halfCupsFlourAdded < 4) || Player.IsHolding(cupHalf),
                autoUnsubscribe: false);
        }

        private void OnFlourClicked_ScoopFlour()
        {
            Player.Debug_LogHeldItems();
            var cupHalf = Items.MeasuringCupHalf;
            var cupOne = Items.MeasuringCupOne;
            if (Player.IsHolding(cupHalf))
            {
                halfCupsFlourAdded++;
                Items.Sieve.SetPowderLevel_AsCups(halfCupsFlourAdded / 2f);
                Items.FlourJar.UpdateHighlights();
            }
            else if (Player.IsHolding(cupOne))
            {
                if (halfCupsFlourAdded >= 4) return;
                halfCupsFlourAdded += 2;
                Items.Sieve.SetPowderLevel_AsCups(halfCupsFlourAdded / 2f);
                Items.FlourJar.UpdateHighlights();
            }

            // No use for the full measuring cup at this point, so prevent user from picking it up.
            if (halfCupsFlourAdded == 4)
                Items.MeasuringCupOne.RemoveClickListener(OnMeasuringCupOneClicked);

            if (halfCupsFlourAdded > 0 && halfCupsFlourAdded < 5)
            {
                string hint =
                    $"Added {Mathf.FloorToInt(halfCupsFlourAdded / 2f)}{(halfCupsFlourAdded % 2 == 0 ? "" : ".5")} cup{(halfCupsFlourAdded == 2 ? "" : "s")}";
                if (hints.Count == 0) hints.Add(hint);
                else hints[0] = hint;
                Game.UpdateUi();
                return;
            }

            Complete();
        }

        private void OnMeasuringCupHalfClicked()
        {
            var cupOne = Items.MeasuringCupOne;
            if (Player.IsHolding(cupOne))
                Player.PutDownItem(cupOne, cupOne.positionBenchPrepArea);

            if (Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND))
            {
                Player.PickUpItem(Items.MeasuringCupHalf);
                Items.FlourJar.UpdateHighlights();
            }
        }

        private void OnMeasuringCupOneClicked()
        {
            var cupHalf = Items.MeasuringCupHalf;
            if (Player.IsHolding(cupHalf))
                Player.PutDownItem(cupHalf, cupHalf.positionBenchPrepArea);

            if (Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND))
            {
                Player.PickUpItem(Items.MeasuringCupOne);
                Items.FlourJar.UpdateHighlights();
            }
        }

        public override void DoTeardown()
        {
            Items.FlourJar.RemoveClickListener(OnFlourClicked_OpenJar);
            Items.FlourJar.RemoveClickListener(OnFlourClicked_ScoopFlour);
            Items.MeasuringCupHalf.RemoveClickListener(OnMeasuringCupHalfClicked);
            Items.MeasuringCupOne.RemoveClickListener(OnMeasuringCupOneClicked);
        }

        public override void SetState_TaskComplete()
        {
            var flourJar = Items.FlourJar;
            flourJar.Lid.SetPlacement(flourJar.Lid.positionBenchPrepArea);
            flourJar.SetLidAttached(false);
            Items.Sieve.SetPowderLevel_AsCups(2.5f);
            hints.Clear();
            
            // If this task is completed normally, then the player should always
            // end with one cup in hand and one on the bench. This is an assumption
            // the next Task relies upon.
            // But if we skip to the end of this task (e.g. with debug commands),
            // this might not be the case. In which case, just pick up one cup
            // and put the other down (doesn't matter which; the choice is arbitrarily).
            var cupOne = Items.MeasuringCupOne;
            var cupHalf = Items.MeasuringCupHalf;
            if (!Player.IsHolding(cupOne) && !Player.IsHolding(cupHalf))
            {
                Player.PickUpItem(cupHalf, skipPickupAnimation: true);
                cupOne.SetPlacement(cupOne.positionBenchPrepArea);
            }
        }
    }
    private class Group4_SubAddBakingSoda : Subtask
    {
        public override string description => "Add 1 teaspoon of baking soda";
        protected override void SetState_TaskStart()
        {
            AddClickListener(Items.BenchPrepArea, OnBenchClicked_PlaceMeasuringCup);
        }

        private void OnBenchClicked_PlaceMeasuringCup()
        {
            var cupHalf = Items.MeasuringCupHalf;
            if (Player.IsHolding(cupHalf))
                Player.PutDownItem(cupHalf, cupHalf.positionBenchPrepArea);
            else
                Player.PutDownItem(Items.MeasuringCupOne, Items.MeasuringCupOne.positionBenchPrepArea);
            AddClickListener(Items.BakingSoda, OnSodaClicked_RemoveLid, requirementOverride: Interactable.Requirement.REQUIRES_BOTH_HANDS);
        }

        private void OnSodaClicked_RemoveLid()
        {
            var bakingSoda = Items.BakingSoda;
            bakingSoda.Lid.SetPlacement(bakingSoda.Lid.positionBenchPrepArea);
            bakingSoda.SetLidAttached(false);
            AddClickListener(Items.MeasuringSpoon, OnSpoonClicked);
        }

        private void OnSpoonClicked()
        {
            Items.MeasuringSpoon.RemoveClickListener(OnSpoonClicked);
            Player.PickUpItem(Items.MeasuringSpoon, onAnimationEndCallback: () =>
            {
                AddClickListener(Items.BakingSoda, OnSodaClicked_ScoopSoda);
            });
        }

        private void OnSodaClicked_ScoopSoda()
        {
            // TODO scoop play animation, wait for animation to end.
            AddClickListener(Items.Sieve, OnSieveClicked);
        }

        private void OnSieveClicked()
        {
            Items.Sieve.SetPowderLevel_AsFraction(1);
            AddClickListener(Items.BenchPrepArea, OnBenchClicked_PlaceSpoon);
        }

        private void OnBenchClicked_PlaceSpoon()
        {
            var spoon = Items.MeasuringSpoon;
            Player.PutDownItem(spoon, spoon.positionBenchPrepArea, onAnimationEndCallback: Complete);
        }

        public override void DoTeardown()
        {
            Items.BenchPrepArea.RemoveClickListener(OnBenchClicked_PlaceMeasuringCup);
            Items.BenchPrepArea.RemoveClickListener(OnBenchClicked_PlaceSpoon);
            Items.BakingSoda.RemoveClickListener(OnSodaClicked_RemoveLid);
            Items.BakingSoda.RemoveClickListener(OnSodaClicked_ScoopSoda);
            Items.MeasuringSpoon.RemoveClickListener(OnSpoonClicked);
            Items.Sieve.RemoveClickListener(OnSieveClicked);
        }

        public override void SetState_TaskComplete()
        {
            var soda = Items.BakingSoda;
            soda.Lid.SetPlacement(soda.Lid.positionBenchPrepArea);
            soda.SetLidAttached(false);
            // This task assumes you start with a measuring cup in hand, and will
            // place it down during the course of the task.
            // So if we skip to the end of the task (e.g. via debug controls)
            // then we need to ensure the cup is placed down.
            var cupHalf = Items.MeasuringCupHalf;
            if (Player.IsHolding(cupHalf))
                Player.PutDownItem(cupHalf, cupHalf.positionBenchPrepArea, skipAnimation: true);
            var cupOne = Items.MeasuringCupOne;
            if (Player.IsHolding(cupOne))
                Player.PutDownItem(cupOne, cupOne.positionBenchPrepArea, skipAnimation: true);
            Items.Sieve.SetPowderLevel_AsFraction(1);
        }
    }
    private class Group4_SubSiftIngredients : Subtask
    {
        public override string description => "Gently shake sieve until ingredients are sifted";
        protected override void SetState_TaskStart()
        {
            AddClickListener(Items.Sieve, OnSieveClicked);
        }

        private void OnSieveClicked()
        {
            Items.Sieve.RemoveClickListener(OnSieveClicked);
            Game.instance.sieveShakeInteraction.Start(Complete);
        }

        public override void DoTeardown()
        {
            Items.Sieve.RemoveClickListener(OnSieveClicked);
        }

        public override void SetState_TaskComplete()
        {
            var sieve = Items.Sieve;
            sieve.SetPowderLevel_AsFraction(0);
            // TODO take into account spillage. Actual powder level will usually be slightly less than 1.
            Items.MixingBowlMedium.SetPowderLevel(1);
            
            if (!Player.IsHolding(sieve))
                Player.PickUpItem(sieve, skipPickupAnimation: true);
        }
    }
    #endregion
    #region Group 5 Soften Butter
    private class Group5_SoftenButter : ParentTask
    {
        public override string description => "Soften butter";

        public override void Save(Dictionary data)
        {
            // TODO
        }

        public override void Load(Dictionary data)
        {
            // TODO
        }

        protected override void SetState_TaskStart()
        {
            var measureTask = new Group5_MeasureButter();
            measureTask.onCompleteCallback += OnButterMeasured;
            subtasks.Add(measureTask);
        }

        private void OnButterMeasured()
        {
            // TODO
        }
    }
    private class Group5_MeasureButter : Subtask
    {
        public override string description => "Measure butter";

        protected override void SetState_TaskStart()
        {
            // TODO
        }
        
        public override void DoTeardown()
        {
            // TODO
        }

        public override void SetState_TaskComplete()
        {
            // TODO
        }
    }
    #endregion
}