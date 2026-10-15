using System;
using System.Linq;
using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Pickupable : Interactable
{
    [Export] private Node3D[] locationNodes;
    [Export] private string[] locationNames;
    private Location[] locations;
    
    [Export] private Node3D initialPosition; // TODO delete this

    public Location location { get; private set; }

    public override void _Ready()
    {
/*        if (locationNodes == null || locationNames == null)
            throw new Exception($"{Name}.locationNodes or .locationNames is null (should never be null).");
        // Check if the list of locations and names are valid. Convert string names to enums.
        if (locationNodes.Length != locationNames.Length)
            throw new Exception($"{Name} has an unequal number of location nodes and location names.");
        locations = new Location[locationNodes.Length];
        var validLocationNames = Enum.GetNames(typeof(Location));
        for (int i = 0; i < locationNodes.Length; i++)
        {
            string locationName = locationNames[i];
            if (string.IsNullOrEmpty(locationName))
                throw new Exception($"{Name} has a null or empty location name.");
            if (!validLocationNames.Contains(locationName))
                throw new Exception($"{Name}'s location \"{ locationName }\" does not match any in the list of valid location names.");
            locations[i] = Enum.Parse<Location>(locationName);
        }
        */
        base._Ready();
    }

    public void SetLocation(Location newLocation)
    {
        Node3D locationNode = null;
        for (int i = 0; i < locationNodes.Length; i++)
        {
            if (locations[i] == location)
            {
                locationNode = locationNodes[i];
                break;
            }
        }
        if (locationNode == null)
            throw new Exception($"Tried to set location to {newLocation}, but no matching Node3D was found.");
        SetPlacement(locationNode);
        location = newLocation;
    }

    protected override void _ResetToGameStartState()
    {
        // Reset position and rotation to fridge starting state.
        SetPlacement(initialPosition);
    }
}