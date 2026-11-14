using System;
using System.Diagnostics;
using Godot;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

public partial class SieveShakeInteraction : Node3D
{
    /// <summary>Max distance to the left or right the sieve is allowed to move.</summary>
    private const float X_MOVEMENT_RANGE = 0.06f;
    
    private bool enabled = false;
    private Sieve sieve;
    private MixingBowlMedium bowl;
    private (Vector3, Vector2) playerInitialPositionAndRotation;
    private Action onCompleteCallback;
    
    private float globalMinXPosition;
    private float globalMaxXPosition;
    private float flourRemainingFraction;
    private float velocityThisFrame;
    private float velocityLastFrame;
    private float acceleration;
    
    /*
     * - UI
     *      - Show text "Gently shake the sieve by moving the mouse side to side"
     *      - Show mouse icon with arrows on either side, possibly with some alternating animation?
     * - Game Logic
     *      - Keep track of how much is spilled.
     * - Particles / Graphics
     *      - Show "splash" / "dust cloud" particles if moving too fast
     *      - Show powder build up on bench around the bowl. Decals?
     */

    public void Start(Action onCompleteCallback)
    {
        sieve = Items.Sieve;
        bowl = Items.MixingBowlMedium;
        this.onCompleteCallback = onCompleteCallback;
        flourRemainingFraction = 1;
        
        // Remove normal player controls, and set camera position.
        playerInitialPositionAndRotation = Player.GetPositionAndRotation();
        Player.EnableNormalControls(false);
        Player.MoveTo(
            position: new Vector3(-0.717f, -0.15f, 0.53f),
            rotation: new Vector2(-32, 180),
            animate: true);
        
        // Position the sieve to hover above the bowl.
        Tween tween = Game.CreateTween();
        tween.SetParallel(true);
        Vector3 sieveHoverPosition = sieve.Position - Vector3.Up * 0.1f; // Sieve mesh is upside down initially, then flipped up when in the bowl. Hence, negative Y offset moves it up.
        tween.TweenProperty(sieve, "position", sieveHoverPosition, 0.5f);
        //tween.TweenProperty(sieve, "quaternion", bodyRotation, 0.5f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(() =>
        {
            // Set and save some values based on position and rotation at the end of the
            // tween. Allows us to dynamically determine important constants even if the
            // animation details change.
            globalMaxXPosition = sieve.GlobalPosition.X + X_MOVEMENT_RANGE;
            globalMinXPosition = sieve.GlobalPosition.X - X_MOVEMENT_RANGE;
            // Set the flour emission's direction to "global left" (need to do some vector math to transform global left to local direction).
            Vector3 localAccelerationVector = sieve.Transform.Basis.Inverse() * Vector3.Left;
            sieve.particlesFalling.ProcessMaterial.Set("direction", localAccelerationVector);
            enabled = true;
        }));
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!enabled || @event is not InputEventMouseMotion motionEvent) return;
        velocityThisFrame = motionEvent.Relative.X;
        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        if (!enabled) return;

        // Move sieve
        float newGlobalXPosition = sieve.GlobalPosition.X - (velocityThisFrame / 3000f);
        float newGlobalXClamped = Mathf.Clamp(newGlobalXPosition, globalMinXPosition, globalMaxXPosition);
        sieve.GlobalPosition = new Vector3(newGlobalXClamped, sieve.GlobalPosition.Y, sieve.GlobalPosition.Z);
        
        // TODO maybe make this exponential? Like, 0-20% movement = 0 emission.
        // medium movement = medium emission.
        // high movement = way too much emission.
        
        // Set particle rate based on acceleration and how much flour is remaining.
        acceleration = velocityThisFrame - velocityLastFrame;
        // More flour remaining = more pressure = more flour ejected at a time.
        // Hence, we scale the output based on flour remaining.
        // 100% full => 100% emission
        //   0% full =>  40% emission
        // TODO take into account (Time.)delta - these values are currently based on framerate!
        float pressure = Mathf.Abs(acceleration) * Mathf.Lerp(1f, 0.4f, flourRemainingFraction);
        var flourFractionEmitted = pressure / 5000f;
        flourRemainingFraction = Mathf.Clamp(flourRemainingFraction - flourFractionEmitted, 0, 1);
        sieve.particlesFalling.AmountRatio = pressure / 5f;
        sieve.particlesCloud.AmountRatio = pressure / 5f;
        sieve.SetPowderLevel_AsFraction(flourRemainingFraction);
        
        // TODO calculate amount of spillage vs amount that actually goes into the bowl.
        // Remember to include a delay between flour being emitted as particles and flour actually piling up in the
        // bowl.
        bowl.SetPowderLevel(1 - flourRemainingFraction);
        // TODO show spillage splashing on to the bench.
        // TODO have particles collide with bowl and get destroyed. See https://docs.godotengine.org/en/stable/classes/class_gpuparticlescollisionsdf3d.html
        
        // Set particle direction and velocity.
        float initialVelocity = -velocityThisFrame / 300f; // Emission direction is already set to "left", so we just set scale and positive/negative.
        sieve.particlesFalling.ProcessMaterial.Set("initial_velocity_min", initialVelocity * 0.9f);
        sieve.particlesFalling.ProcessMaterial.Set("initial_velocity_max", initialVelocity * 1.1f);
        
        // Track velocity across multiple frames, so we can track acceleration.
        velocityLastFrame = velocityThisFrame;
        velocityThisFrame = 0;
        if (flourRemainingFraction <= 0)
            OnAllFlourSifted();
    }
    
    private void OnAllFlourSifted()
    {
        enabled = false;

        sieve.particlesFalling.AmountRatio = 0;
        sieve.particlesCloud.AmountRatio = 0;

        Player.MoveTo(
            position: playerInitialPositionAndRotation.Item1,
            rotation: playerInitialPositionAndRotation.Item2,
            animate: true,
            onAnimationEndCallback: () =>
            {
                Player.PickUpItem(sieve);
                Player.EnableNormalControls(true);
                onCompleteCallback?.Invoke();
            });
    }
}