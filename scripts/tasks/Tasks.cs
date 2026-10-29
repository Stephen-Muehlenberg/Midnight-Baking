using System.Collections.Generic;
using System.Diagnostics;
using Godot;

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
            Items.LightSwitch.AddClickListener(Complete);
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
            Items.Apron.AddClickListener(Complete);
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
            Items.Sink.AddClickListener(OnSinkClicked);
        }

        private void OnSinkClicked()
        {
            GD.Print("OnSinkClicked()");
            Items.Sink.RemoveClickListener(OnSinkClicked);
            Items.Soap.AddClickListener(OnSoapClicked);
            hints.Add("Use soap");
            Game.UpdateUi();
        }

        private void OnSoapClicked()
        {
            Items.Soap.RemoveClickListener(OnSoapClicked);
            Items.Towel.AddClickListener(Complete);
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
            AddClickListener(Items.Radio, OnRadioClicked);
            stationsRemaining = 5;
            timer = null;
        }

        private void OnRadioClicked()
        {
            stationsRemaining--;

            if (stationsRemaining > 0)
            {
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
            else Complete();
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
            fridge.RemoveClickListener(OnFridgeClicked);
            AddClickListener(fridge, OnFridgeClicked, highlight: !fridge.isOpen, Interactable.Requirement.REQUIRES_ONE_HAND);
        }

        private void OnButterClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND))
                return;
            
            Items.Fridge.RemoveClickListener(OnFridgeClicked);
            var butter = Items.Butter;
            butter.RemoveClickListener(OnButterClicked);
            Player.HoldItem(butter);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            fridge.RemoveClickListener(OnFridgeClicked);
            AddClickListener(fridge, OnFridgeClicked, highlight: () => !fridge.isOpen, Interactable.Requirement.REQUIRES_ONE_HAND);
        }

        private void OnCartonPickedUp()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.Fridge.RemoveClickListener(OnFridgeClicked);
            var eggCarton = Items.EggCarton;
            eggCarton.RemoveClickListener(OnCartonPickedUp);
            Player.HoldItem(eggCarton);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            cupboard.RemoveClickListener(OnCupboardClicked);
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);

        }

        private void OnFlourClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var flour = Items.FlourJar;
            flour.RemoveClickListener(OnFlourClicked);
            Player.HoldItem(flour);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            cupboard.RemoveClickListener(OnCupboardClicked);
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnSugarClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Items.SugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            Player.HoldItem(sugar);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            cupboard.RemoveClickListener(OnCupboardClicked);
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);

        }

        private void OnSugarClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var sugar = Items.BrownSugarJar;
            sugar.RemoveClickListener(OnSugarClicked);
            Player.HoldItem(sugar);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            cupboard.RemoveClickListener(OnCupboardClicked);
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnSodaClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var soda = Items.BakingSoda;
            soda.RemoveClickListener(OnSodaClicked);
            Player.HoldItem(soda);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            cupboard.RemoveClickListener(OnCupboardClicked);
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnVanillaClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var vanilla = Items.VanillaJar;
            vanilla.RemoveClickListener(OnVanillaClicked);
            Player.HoldItem(vanilla);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            cupboard.RemoveClickListener(OnCupboardClicked);
            AddClickListener(cupboard, OnCupboardClicked, highlight: () => !cupboard.isOpen);
        }

        private void OnChocClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            Items.CupboardUpper.RemoveClickListener(OnCupboardClicked);
            var chocChips = Items.ChocChipBag;
            chocChips.RemoveClickListener(OnChocClicked);
            Player.HoldItem(chocChips);
            AddClickListener(Items.BenchPrepArea, Complete);
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
            Items.Oven.TemperatureDial.AddClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            var oven = Items.Oven;
            oven.TemperatureDial.RemoveClickListener(Complete);
            oven.SetOn(true);
        }
    }
    #endregion
    #region Group 4 Sift Dry Ingredients
    private class Group4_SiftDryIngredients : ParentTask
    {
        private int cupsAndSievePlacedCount = 0;
        
        // TODO allow user to set metrics vs imperial in the menu
        public override string description => "Sift flour and bicarb soda into a medium bowl";

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
            flourTask.onCompleteCallback += Complete; // TODO this isn't actually the end; add the next step when ready.
            AddSubtask(flourTask, startSubtask: true);
        }

        public override void SetState_TaskComplete()
        {
            // TODO
        }
    }
    private class Group4_SubCups : Subtask
    {
        public override string description => "Get measuring cups (1 cup and 1/2 cup)";
        private int cupsPickedUp;
        private int cupsPlaced;

        protected override void SetState_TaskStart()
        {
            cupsPickedUp = 0;
            cupsPlaced = 0;
            var drawer = Items.DrawerUtensils;
            drawer.AddClickListener(OnDrawerClicked, highlight: !drawer.IsOpen);
            Items.MeasuringCupOne.AddClickListener(OnCupOneClicked);
            Items.MeasuringCupHalf.AddClickListener(OnCupHalfClicked);
        }

        private void OnDrawerClicked()
        {
            var drawer = Items.DrawerUtensils;
            drawer.RemoveClickListener(OnDrawerClicked);
            drawer.AddClickListener(OnDrawerClicked, highlight: !drawer.IsOpen && cupsPickedUp < 2);
        }

        private void OnCupOneClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;

            cupsPickedUp++;
            var cup = Items.MeasuringCupOne;
            cup.RemoveClickListener(OnCupOneClicked);
            Player.HoldItem(cup);
            Items.BenchPrepArea.AddClickListener(OnFullCupPlaced);
        }

        private void OnCupHalfClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;

            cupsPickedUp++;
            var cup = Items.MeasuringCupHalf;
            cup.RemoveClickListener(OnCupHalfClicked);
            Player.HoldItem(cup);
            Items.BenchPrepArea.AddClickListener(OnHalfCupPlaced);
        }

        private void OnFullCupPlaced()
        {
            cupsPlaced++;
            if (cupsPlaced == 1)
            {
                var bench = Items.BenchPrepArea;
                bench.RemoveClickListener(OnFullCupPlaced);
                var cup = Player.heldItemLeft.id == ItemId.MeasuringCup_One
                    ? Player.heldItemLeft
                    : Player.heldItemRight;
                cup.SetPlacement(bench.measuringCupOnePlacement);
            }
            else Complete();
        }

        private void OnHalfCupPlaced()
        {
            cupsPlaced++;
            if (cupsPlaced == 1)
            {
                var bench = Items.BenchPrepArea;
                bench.RemoveClickListener(OnHalfCupPlaced);
                var cup = Player.heldItemLeft.id == ItemId.MeasuringCup_Half
                    ? Player.heldItemLeft
                    : Player.heldItemRight;
                cup.SetPlacement(bench.measuringCupHalfPlacement);
            }
            else Complete();
        }

        public override void DoTeardown()
        {
            Items.DrawerUtensils.RemoveClickListener(OnDrawerClicked);
            Items.BenchPrepArea.RemoveClickListener(OnFullCupPlaced);
            Items.BenchPrepArea.RemoveClickListener(OnHalfCupPlaced);
            Items.MeasuringCupOne.RemoveClickListener(OnCupOneClicked);
            Items.MeasuringCupHalf.RemoveClickListener(OnCupHalfClicked);
        }

        public override void SetState_TaskComplete()
        {
            var bench = Items.BenchPrepArea;
            Items.MeasuringCupOne.SetPlacement(bench.measuringCupOnePlacement);
            Items.MeasuringCupHalf.SetPlacement(bench.measuringCupHalfPlacement);
        }
    }
    private class Group4_SubBowl : Subtask
    {
        public override string description => "Get medium bowl";

        protected override void SetState_TaskStart()
        {
            Items.MixingBowlMedium.AddClickListener(OnBowlClicked);
        }

        private void OnBowlClicked()
        {
            var bowl = Items.MixingBowlMedium;
            bowl.RemoveClickListener(OnBowlClicked);
            Player.HoldItem(bowl);
            Items.BenchPrepArea.AddClickListener(Complete);
        }

        public override void DoTeardown()
        {
            Items.MixingBowlMedium.RemoveClickListener(OnBowlClicked);
            Items.BenchPrepArea.RemoveClickListener(Complete);
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
            Items.Sieve.AddClickListener(OnSieveClicked);
        }

        private void OnSieveClicked()
        {
            if (!Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND)) return;
            
            var sieve = Items.Sieve;
            sieve.RemoveClickListener(OnSieveClicked);
            Player.HoldItem(sieve);
            Items.MixingBowlMedium.AddClickListener(Complete, interactionRequirement: Interactable.Requirement.NO_REQUIREMENT);
        }

        public override void DoTeardown()
        {
            Items.Sieve.RemoveClickListener(OnSieveClicked);
            Items.MixingBowlMedium.RemoveClickListener(Complete);
        }

        public override void SetState_TaskComplete()
        {
            Items.Sieve.SetPlacement(Items.MixingBowlMedium.sievePivot);
        }
    }
    private class Group4_SubAddFlour : Subtask
    {
        private bool lidRemoved;
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

            flourJar.RemoveClickListener(OnFlourClicked_OpenJar);
            flourJar.Lid.SetPlacement(flourJar.Lid.positionBenchPrepArea);
            
            AddClickListener(cupHalf, OnMeasuringCupHalfClicked, highlight: () =>
            {
                if (Player.IsHolding(cupHalf)) return false;
                return Player.IsHolding(cupOne) || Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND);
            });
            AddClickListener(cupOne, OnMeasuringCupOneClicked, highlight: () =>
            {
                if (Player.IsHolding(cupOne)) return false;
                return Player.IsHolding(cupHalf) || Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND);
            });
            AddClickListener(flourJar, OnFlourClicked_ScoopFlour,
                highlight: () => (Player.IsHolding(cupOne) && halfCupsFlourAdded < 4) || Player.IsHolding(cupHalf));
        }

        private void OnFlourClicked_ScoopFlour()
        {
            var cupHalf = Items.MeasuringCupHalf;
            var cupOne = Items.MeasuringCupOne;
            if (Player.IsHolding(cupHalf))
                halfCupsFlourAdded++;
            else if (Player.IsHolding(cupOne))
            {
                if (halfCupsFlourAdded >= 4) return;
                halfCupsFlourAdded += 2;
            }
            
            if (halfCupsFlourAdded < 5)
            {
                string hint = $"Added {Mathf.FloorToInt(halfCupsFlourAdded / 2f)}{(halfCupsFlourAdded % 2 == 0 ? "" : ".5")} cup{(halfCupsFlourAdded == 2 ? "" : "s")}";
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
            {
                Player.StopHoldingItem(cupOne);
                cupOne.SetPlacement(cupOne.positionBenchPrepArea);
            }
            if (Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND))
                Player.HoldItem(Items.MeasuringCupHalf);
        }
        
        private void OnMeasuringCupOneClicked()
        {
            var cupHalf = Items.MeasuringCupOne;
            if (Player.IsHolding(cupHalf))
            {
                Player.StopHoldingItem(cupHalf);
                cupHalf.SetPlacement(cupHalf.positionBenchPrepArea);
            }
            if (Player.CanInteract(Interactable.Requirement.REQUIRES_ONE_HAND))
                Player.HoldItem(Items.MeasuringCupOne);
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
        }
    }
    #endregion
}