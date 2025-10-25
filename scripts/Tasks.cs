using System.Threading.Tasks;
using Godot;

namespace MidnightBaking.scripts;

public static class Tasks
{
    #region Group 0
    public class Group0_MouseLook : Game.Task
    {
        public override string description => "Use the mouse to look around";

        protected override void OnStart()
        {
            FetchPlayerController().OnPlayerLook += OnPlayerLook;
        }

        private void OnPlayerLook()
        {
            FetchPlayerController().OnPlayerLook -= OnPlayerLook;
            Complete();
        }
        
        private PlayerController FetchPlayerController() =>
            // Hardcoded string is gross, but Godot lacks proper type searching.
            Game.instance.GetTree().Root.GetNode<PlayerController>("Level/Player Controller");
    }

    public class Group0_Move : Game.Task
    {
        // TODO dynamically set keys based on keybindings
        public override string description => "Use WASD to move";
        
        protected override void OnStart()
        {
            FetchPlayerController().OnPlayerMove += OnPlayerMove;
        }

        private void OnPlayerMove()
        {
            FetchPlayerController().OnPlayerMove -= OnPlayerMove;
            Complete();
        }
        
        private PlayerController FetchPlayerController() =>
            // Hardcoded string is gross, but Godot lacks proper type searching.
            Game.instance.GetTree().Root.GetNode<PlayerController>("Level/Player Controller");

    }

    public class Group0_Interact : Game.Task
    {
        public override string description => "Use the left mouse button to interact";
        // hint = "Turn on the lights by using the light switch by the door"

        protected override void OnStart()
        {
            hints.Add("Turn on the lights by using the light switch by the door");
            Game.Interactables[Game.ItemId.LIGHT_SWITCH].EnableInteractions(() =>
            {
                Game.Interactables[Game.ItemId.LIGHT_SWITCH].DisableInteractions();
                Complete();
            });
        }
    }
    #endregion
    #region Group 1
    public class Group1_PutOnApron : Game.Task
    {
        public override string description => "Put on apron";
        protected override void OnStart()
        {
  //          Game.Interactables[Game.ItemId.APRON].EnableInteractions(OnApronClicked);
        }

        private void OnApronClicked()
        {
            Game.Interactables[Game.ItemId.APRON].DisableInteractions();
            // TODO Hide the apron
            Complete();
        }
    }

    public class Group1_WashHands : Game.Task
    {
        public override string description => "Wash hands";
        protected override void OnStart()
        {
            hints.Add("Use sink");
            Game.Interactables[Game.ItemId.SINK].EnableInteractions(OnSinkClicked);
        }

        private void OnSinkClicked()
        {
            Game.Interactables[Game.ItemId.SINK].DisableInteractions();
            hints.Add("Dry hands on towel");
            Game.UpdateUi();
            Game.Interactables[Game.ItemId.TOWEL].EnableInteractions(OnTowelClicked);
        }

        private void OnTowelClicked()
        {
            Game.Interactables[Game.ItemId.TOWEL].DisableInteractions();
            Complete();
        }
    }

    public class Group1_OpenRecipe : Game.Task
    {
        public override string description => "Open recipe";

        protected override void OnStart()
        {
            hints.Add("Click on the tablet");
            Game.Interactables[Game.ItemId.TABLET].EnableInteractions(() =>
            {
                Game.Interactables[Game.ItemId.TABLET].DisableInteractions();
                Complete();
            });
        }
    }
    #endregion
    #region Group 2
    public class Group2_GatherIngredients : Game.Task
    {
        public override string description => "Gather ingredients";
        
    }
    #endregion
}