using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Apron : Interactable
{
    [Export] private Node3D apronModel;
    [Export] private Area3D apronCollider;
    
    protected override void _ResetToGameStartState()
    {
        SetApronVisible(true);
    }

    public void SetApronVisible(bool visible)
    {
        apronModel.Visible = visible;
        apronCollider.CollisionLayer = (uint) (visible ? 1 : 0);
    }
}