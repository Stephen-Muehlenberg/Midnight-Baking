using Godot;
using Godot.Collections;

namespace MidnightBaking.scripts.interactables;

public partial class Apron : Interactable
{
    [Export] private Node3D apronModel;
    [Export] private Area3D apronCollider;
    private bool apronPickedUp;
    
    protected override void _ResetToGameStartState()
    {
        SetApronVisible(true);
    }

    public override void Save(Dictionary data)
    {
        data["pickedUp"] = apronPickedUp;
    }

    public override void Load(Dictionary data)
    {
        SetApronVisible(!data["pickedUp"].AsBool());
    }

    public void SetApronVisible(bool visible)
    {
        apronPickedUp = !visible;
        apronModel.Visible = visible;
        apronCollider.CollisionLayer = (uint) (visible ? 1 : 0);
    }
}