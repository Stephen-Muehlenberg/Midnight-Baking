using System;
using Godot;
using Godot.Collections;

namespace MidnightBaking.scripts.interactables;

public partial class Apron : Interactable
{
    private const string SaveKey_PickedUp = "apron_pickedUp";
    [Export] private Node3D apronModel;
    [Export] private Area3D apronCollider;
    private bool apronPickedUp = false;
    
    protected override void _ResetToGameStartState()
    {
        SetApronVisible(true);
    }

    public override void LoadState(Dictionary json)
    {
        SetApronVisible(LoadBool(json, SaveKey_PickedUp));
    }

    private bool LoadBool(Dictionary json, string key) => LoadValue<bool>(json, key, Variant.Type.Bool);
    private float LoadFloat(Dictionary json, string key) => LoadValue<float>(json, key, Variant.Type.Float);

    private T LoadValue<[MustBeVariant] T>(Dictionary json, string key, Variant.Type type)
    {
        if (!json.TryGetValue(key, out var value))
            throw new Exception($"Json parse error: \"{key}\" not found.");
        if (value.VariantType != Variant.Type.Float)
            throw new Exception($"Json parse error: \"{key}\" expected type {type} but was {value.VariantType}.");
        return value.As<T>();
    }

    public override void SaveState(Dictionary json)
    {
        json.Add(SaveKey_PickedUp, apronPickedUp);
//        json[SaveKey_PickedUp] = apronPickedUp;
    }

    public void SetApronVisible(bool visible)
    {
        apronPickedUp = !visible;
        apronModel.Visible = visible;
        apronCollider.CollisionLayer = (uint) (visible ? 1 : 0);
    }
}