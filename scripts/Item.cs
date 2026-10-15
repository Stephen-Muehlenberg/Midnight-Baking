using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

/// <summary>
/// Central directory for all <see cref="Interactable"/>s. Provides IDs and public static links for each
/// Interactable, making them easy to access and uniquely identify.
/// </summary>
public partial class Item : Node
{
    public enum Id
    {
        /// <summary>UNDEFINED should never be used. As the default value, it is used to identify Ids which have not
        /// been correctly initialised to other valid values. This helps avoid the case where multiple items might
        /// initialise themselves to, e.g. "Apron", and you have to manually figure out which is deliberately "Apron"
        /// and which are just using the default value.</summary>
        UNDEFINED,
        Apron,
        BakingSoda_Jar, BenchPrepArea, BrownSugar_Jar, Butter,
        ChocChips_Jar, Cupboard_Upper,
        Drawer_Utensils,
        EggCarton,
        FlourJar, Fridge,
        KitchenLight_Switch,
        Measuring_FullCup, Measuring_HalfCup, Measuring_Teaspoon, Microwave, MixingBowl_Medium,
        Oven, Oven_Dial, Oven_Door,
        PantryDoor,
        Radio,
        Sieve, Sink, Soap,
        Tablet, Towel,
        VanillaJar,
        WhiteSugar_Jar,
    }
    
    // Fixtures
    public static BenchPrepArea BenchPrepArea => All[Id.BenchPrepArea] as BenchPrepArea;
    public static Door CupboardUpper => All[Id.Cupboard_Upper] as Door;
    public static Drawer DrawerUtensils => All[Id.Drawer_Utensils] as Drawer;
    public static KitchenLightController LightSwitch => All[Id.KitchenLight_Switch] as KitchenLightController;
    public static Door PantryDoor => All[Id.PantryDoor] as Door;
    public static Interactable Sink => All[Id.Sink];
    
    // Appliances
    public static Fridge Fridge => All[Id.Fridge] as Fridge;
    public static Microwave Microwave => All[Id.Microwave] as Microwave;
    public static OvenController Oven;
    public static Tablet Tablet => All[Id.Tablet] as Tablet;

    // Ingredients
    public static Pickupable BakingSoda => All[Id.BakingSoda_Jar] as Pickupable;
    public static Pickupable BrownSugarJar => All[Id.BrownSugar_Jar] as Pickupable;
    public static Pickupable ChocChipBag => All[Id.ChocChips_Jar] as Pickupable;
    public static Interactable Butter => All[Id.Butter];
    public static EggCarton EggCarton => All[Id.EggCarton] as EggCarton;
    public static FlourJar FlourJar => All[Id.FlourJar] as FlourJar;
    public static Interactable SugarJar => All[Id.WhiteSugar_Jar];
    public static Pickupable VanillaJar => All[Id.VanillaJar] as Pickupable;

    // Utensils
    public static Apron Apron => All[Id.Apron] as Apron;
    public static Pickupable MeasuringCupOne => All[Id.Measuring_FullCup] as Pickupable;
    public static Pickupable MeasuringCupHalf => All[Id.Measuring_HalfCup] as Pickupable;
    public static Pickupable MeasuringSpoon => All[Id.Measuring_Teaspoon] as Pickupable;
    public static MixingBowlMedium MixingBowlMedium => All[Id.MixingBowl_Medium] as MixingBowlMedium;
    public static Pickupable Sieve => All[Id.Sieve] as Pickupable;
    
    // Other
    public static Interactable Soap => All[Id.Soap];
    public static Interactable Towel => All[Id.Towel];

    public static readonly Dictionary<Id, Interactable> All = new();

    /// <summary>
    /// Links an interactable object in the scene with its corresponding Item.Id.
    /// Should be invoked by all <see cref="Interactable"/>s during their <see cref="_Ready()"/>.
    /// Returns the object's <see cref="Item.Id"/>, for the object to save to itself.
    /// </summary>
    public static Id RegisterInteractable(Interactable interactable)
    {
        var interactableIds = Enum.GetNames(typeof(Id));
        string simpleName = interactable.Name.ToString().Replace(" ", "").Replace("_", "").ToLower();
        for (int i = 0; i < interactableIds.Length; i++)
            if (simpleName == interactableIds[i].Replace("_", "").ToLower())
            {
                All.Add((Id) i, interactable);
                return (Id) i;
            }
        throw new Exception($"Interactable \"{interactable.Name}\" does not correspond to any interactable id. Ensure the name matches an existing id, or add a new id. Existing ids:\n{interactableIds.Stringify()}");
    }
}