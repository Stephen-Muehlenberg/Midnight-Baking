using System;
using System.Collections.Generic;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

/// <summary>
/// Central directory for all <see cref="Interactable"/>s. Provides public static links for each
/// Interactable, making them easy to access and uniquely identify.
/// </summary>
public static class Items
{
    // Fixtures
    public static BenchPrepArea BenchPrepArea => All[ItemId.BenchPrepArea] as BenchPrepArea;
    public static Door CupboardUpper => All[ItemId.Cupboard_Upper] as Door;
    public static Drawer DrawerUtensils => All[ItemId.Drawer_Utensils] as Drawer;
    public static KitchenLightController LightSwitch => All[ItemId.KitchenLight_Switch] as KitchenLightController;
    public static Door PantryDoor => All[ItemId.PantryDoor] as Door;
    public static Interactable Sink => All[ItemId.Sink];
    
    // Appliances
    public static Fridge Fridge => All[ItemId.Fridge] as Fridge;
    public static Microwave Microwave => All[ItemId.Microwave] as Microwave;
    public static OvenController Oven;
    public static Radio Radio => All[ItemId.Radio] as Radio;
    public static Tablet Tablet => All[ItemId.Tablet] as Tablet;

    // Ingredients
    public static BakingSoda BakingSoda => All[ItemId.BakingSoda_Jar] as BakingSoda;
    public static BrownSugar BrownSugarJar => All[ItemId.BrownSugar_Jar] as BrownSugar;
    public static ChocChipsJar ChocChipBag => All[ItemId.ChocChips_Jar] as ChocChipsJar;
    public static Butter Butter => All[ItemId.Butter] as Butter;
    public static EggCarton EggCarton => All[ItemId.EggCarton] as EggCarton;
    public static FlourJar FlourJar => All[ItemId.Flour_Jar] as FlourJar;
    public static WhiteSugar SugarJar => All[ItemId.WhiteSugar_Jar] as WhiteSugar;
    public static VanillaJar VanillaJar => All[ItemId.VanillaJar] as VanillaJar;

    // Utensils
    public static Apron Apron => All[ItemId.Apron] as Apron;
    public static MeasuringCupOne MeasuringCupOne => All[ItemId.MeasuringCup_One] as MeasuringCupOne;
    public static MeasuringCupHalf MeasuringCupHalf => All[ItemId.MeasuringCup_Half] as MeasuringCupHalf;
    public static MeasuringSpoon MeasuringSpoon => All[ItemId.Measuring_Teaspoon] as MeasuringSpoon;
    public static MixingBowlMedium MixingBowlMedium => All[ItemId.MixingBowl_Medium] as MixingBowlMedium;
    public static Sieve Sieve => All[ItemId.Sieve] as Sieve;
    
    // Other
    public static Interactable Soap => All[ItemId.Soap];
    public static Interactable Towel => All[ItemId.Towel];

    public static readonly Dictionary<ItemId, Interactable> All = new();

    /// <summary>
    /// Links an interactable object in the scene with its corresponding ItemId.
    /// Should be invoked by all <see cref="Interactable"/>s during their <see cref="_Ready()"/>.
    /// Returns the object's <see cref="ItemId"/>, for the object to save to itself.
    /// </summary>
    public static ItemId RegisterInteractable(Interactable interactable)
    {
        var interactableIds = Enum.GetNames(typeof(ItemId));
        string simpleName = interactable.Name.ToString().Replace(" ", "").Replace("_", "").ToLower();
        for (int i = 0; i < interactableIds.Length; i++)
            if (simpleName == interactableIds[i].Replace("_", "").ToLower())
            {
                All.Add((ItemId) i, interactable);
                return (ItemId) i;
            }
        throw new Exception($"Interactable \"{interactable.Name}\" does not correspond to any interactable id. Ensure the name matches an existing id, or add a new id. Existing ids:\n{interactableIds.Stringify()}");
    }
}