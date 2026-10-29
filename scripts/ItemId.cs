namespace MidnightBaking.scripts;

/// <summary>
/// Enum identifying individual to <see cref="Interactable"/>s. Every Interactable listed in <see cref="Items"/>
/// should have exactly one corresponding ItemId.
/// </summary>
public enum ItemId
{
    /// <summary>UNDEFINED should never be used. As the default value, it is used to identify Ids which have not
    /// been correctly initialised to other valid values.<br/><br/>This helps avoid the case where multiple items might
    /// initialise themselves to, e.g. "Apron", and you have to manually figure out which is deliberately "Apron"
    /// and which are just using the default value.</summary>
    UNDEFINED,
    Apron,
    BakingSoda_Jar, BenchPrepArea, BrownSugar_Jar, Butter,
    ChocChips_Jar, Cupboard_Upper,
    Drawer_Utensils,
    EggCarton,
    FlourJar, FlourJar_Lid, Fridge,
    KitchenLight_Switch,
    MeasuringCup_One, MeasuringCup_Half, Measuring_Teaspoon, Microwave, MixingBowl_Medium,
    Oven, Oven_Dial, Oven_Door,
    PantryDoor,
    Radio,
    Sieve, Sink, Soap,
    Tablet, Towel,
    VanillaJar,
    WhiteSugar_Jar,
}