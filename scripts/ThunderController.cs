using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MidnightBaking.scripts;

public partial class ThunderController : Node
{
    [Export] public DirectionalLight3D directionalLight;
    [Export] public OmniLight3D omniLight;
    [Export] public AudioStreamPlayer3D thunderAudio;
    [Export] public AudioStream[] thunderSounds;
    
    private RandomNumberGenerator random = new();
    private float timeTillNextLightning;
    private int[] recentThunderAudioIndices = [-1, -1, -1];

    /// <summary>Speed of sound, in metres per second. Used for calculating thunder delay.</summary>
    private const float SPEED_OF_SOUND = 343;
    /// <summary>Maximum distance from kitchen lightning can strike, in metres. Determines the maximum delay between lightning and thunder.</summary>
    private const float MAX_LIGHTNING_DISTANCE = 6.5f * SPEED_OF_SOUND;
    /// <summary>Max lightning intensity, relative to the minimum intensity of 1.</summary>
    private const float MAX_LIGHTNING_INTENSITY = 3;
    /// <summary>Intensity multiplier at distance = 0, relative to multiplier of 1 at distance = max.</summary>
    private const float LIGHTNING_DISTANCE_INTENSITY_MULTIPLIER = 3;
    
    public override void _Ready()
    {
        directionalLight.LightEnergy = 0;
        omniLight.LightEnergy = 0;
        timeTillNextLightning = GetRandomTimeTillNextLightning();
    }

    public override void _Process(double delta)
    {
        timeTillNextLightning -= (float) delta;
        if (timeTillNextLightning <= 0)
            TriggerLightning();
    }
    
    public override void _Input(InputEvent @event)
    {
        // Only listen for left mouse click events.
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.L && keyEvent.Pressed)
                TriggerLightning();
        }
    }

    private float GetRandomTimeTillNextLightning()
    {
        //return random.RandfRange(1, 70);
        return random.RandfRange(1, 50);
    }

    public void TriggerLightning()
    {
        LightningDetails lightning = GenerateRandomLightning();

        bool instantFlash = random.Randf() < 0.9f; // 90% chance of instant flash, 10% of slower flash
        
        // todo audio + delay
        // todo flash + brightness
        // todo screen shake (rare)
        
        LightningTest(lightning.intensity);
//        PlayThunderSound();
        GetTree().CreateTimer(lightning.audioDelay).Timeout += PlayThunderSound;
        
        timeTillNextLightning = GetRandomTimeTillNextLightning();
    }

    private void LightningTest(float intensity)
    {
        directionalLight.LightEnergy = 2*intensity + 0.5f;
        omniLight.LightEnergy = intensity / 9f;
        GetTree().CreateTimer(0.16f).Timeout += () =>
        {
            directionalLight.LightEnergy = 0;
            omniLight.LightEnergy = 0;
        };
    }

    private void PlayThunderSound()
    {
        // Pick a random thunder sound that we haven't played recently.
        int thunderIndex = 0;
        for (int i = 0; i < 20; i++)
        {
            thunderIndex = random.RandiRange(0, thunderSounds.Length - 1);
            // Statistically nearly certain to get a non-recent sound after 20 tries,
            // but if we don't then just accept whichever one we picked on the 20th try.
            if (!recentThunderAudioIndices.Contains(thunderIndex))
                break;
        }
        
        // Track the recent thunder sounds we've played, so we can exclude them from the next few thunders.
        for (int i = recentThunderAudioIndices.Length - 1; i > 0; i--)
            recentThunderAudioIndices[i] = recentThunderAudioIndices[i - 1];
        recentThunderAudioIndices[0] = thunderIndex;

        // Create a new instance of the audio player, so we can have multiple overlapping thunder sounds if necessary.
        var audioSource = thunderAudio.Duplicate() as AudioStreamPlayer3D;
        AddChild(audioSource);
        audioSource.Stream = thunderSounds[thunderIndex];
        audioSource.Play();

        // Queue the audio player instance for deletion once it's finished playing.
        double clipDuration = audioSource.Stream.GetLength();
        GetTree().CreateTimer(clipDuration).Timeout += () =>
        {
            audioSource.QueueFree();
        };
    }

    private LightningDetails GenerateRandomLightning()
    {
        // Constants used in later calculations.
        float maxLightningDistanceSq = Mathf.Pow(MAX_LIGHTNING_DISTANCE, 2);
        float axisDistanceMetres = Mathf.Sin(Mathf.DegToRad(45)) * MAX_LIGHTNING_DISTANCE;

        // Randomize lightning properties.
        Vector2 lightningLocation = default;
        for (int i = 0; i < 20; i++)
        {
            lightningLocation = new Vector2(
                random.RandfRange(-axisDistanceMetres, axisDistanceMetres),
                random.RandfRange(-axisDistanceMetres, axisDistanceMetres));
            // Statistically almost guaranteed to generate valid coordinates within 20 tries.
            // But if we don't, just use 20th coordinates; they'll just be slightly further than intended.
            if (lightningLocation.LengthSquared() <= maxLightningDistanceSq)
                break;
        }
        float lightningIntensity = random.RandfRange(1, MAX_LIGHTNING_INTENSITY);
        
        // Calculate derived values.
        float distance = lightningLocation.Length();
        // Technically distance-based intensity should have an exponential falloff, but for now we're just using linear falloff.
        float distanceIntensity = Mathf.Lerp(LIGHTNING_DISTANCE_INTENSITY_MULTIPLIER, 1, distance / MAX_LIGHTNING_DISTANCE);
        // Maximum possible perceived intensity = 1. Useful for multiplying with other values.
        float totalPerceivedIntensityFraction = (distanceIntensity + lightningIntensity) / (MAX_LIGHTNING_INTENSITY + LIGHTNING_DISTANCE_INTENSITY_MULTIPLIER);
        
        float flashCountRandom = random.RandfRange(0, 100);
        int flashCount = flashCountRandom < 2 ? 3 // 2% chance of 3 flashes
            : flashCountRandom < 18 ? 2 // 16% chance of 2 flashes
            : 1;
        
        return new LightningDetails()
        {
            // If the distance is 0 (or near 0), then getting the normalised vector can be unreliable (dividing by 0).
            // In the very unlikely event that the distance is near 0, just default to a safe valid value.
            direction = distance >= 1
                ? lightningLocation.Normalized()
                : Vector2.Up,
            intensity = totalPerceivedIntensityFraction,
            flashCount = flashCount,
            audioDelay = distance / SPEED_OF_SOUND
        };
    }

    private class LightningDetails
    {
        /// <summary>Direction of the lightning, relative to the kitchen.</summary>
        public Vector2 direction;
        /// <summary>A multiplier, from 0 to 1, indicating the lightning's intensity.</summary>
        public float intensity;
        public int flashCount;
        public float audioDelay;
    }
}