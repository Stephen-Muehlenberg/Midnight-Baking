using System;
using Godot;

namespace MidnightBaking.scripts;

/// <summary>
/// Handles player first person camera and movement.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    public bool canMove = true;
    public bool canLook = true;

    [ExportGroup("Speeds")]
    [Export] public float lookSpeedHorizontal = 0.002f;
    [Export] public float lookSpeedVertical = 0.002f;
    [Export] public bool invertCameraVertical = false;
    [Export] public float moveSpeed = 3.0f;

    [ExportGroup("Input Names")]
    [Export] public string inputName_MoveLeft = "move_left";
    [Export] public string inputName_MoveRight = "move_right";
    [Export] public string inputName_MoveForward = "move_forward";
    [Export] public string inputName_MoveBack = "move_backward";
    
    [Signal] public delegate void OnPlayerLookEventHandler();
    [Signal] public delegate void OnPlayerMoveEventHandler();

    private bool mouseCaptured;
    private Vector2 lookRotation;
    private float currentMoveSpeed;

    private Node3D head;
    private CollisionShape3D collider;

    private readonly float MIN_VERTICAL_ANGLE_RADIANS = float.DegreesToRadians(-85);
    private readonly float MAX_VERTICAL_ANGLE_RADIANS = float.DegreesToRadians(85);
    
    public override void _Ready()
    {
        head = GetNode<Node3D>("Head");
        collider = GetNode<CollisionShape3D>("Collider");
        CheckInputMappings();
        lookRotation.Y = Rotation.Y;
        lookRotation.X = head.Rotation.X;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Capture input.
        if (!mouseCaptured && Input.IsMouseButtonPressed(MouseButton.Left))
            SetMouseCaptured(true);
        if (Input.IsKeyPressed(Key.Escape))
            SetMouseCaptured(false);
        
        // Look around.
        if (mouseCaptured && @event is InputEventMouseMotion motionEvent)
        {
            RotateLook(motionEvent.Relative);
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Add input to velocity.
        if (canMove)
        {
            Vector2 inputDirection = Input.GetVector(inputName_MoveLeft, inputName_MoveRight, inputName_MoveForward, inputName_MoveBack);
            Vector3 normalisedWorldMovementDirection = (Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();
            if (normalisedWorldMovementDirection != Vector3.Zero)
            {
                Velocity = new Vector3(
                    normalisedWorldMovementDirection.X * moveSpeed,
                    0,
                    normalisedWorldMovementDirection.Z * moveSpeed);
                
                EmitSignalOnPlayerMove();
            }
            else
                Velocity = Velocity.MoveToward(Vector3.Zero, moveSpeed);
        }
        else
            Velocity = Vector3.Zero;
        
        // Move based on velocity.
        MoveAndSlide();
    }
    
    private void RotateLook(Vector2 rotationInput)
    {
        lookRotation = new Vector2(
            Mathf.Clamp(rotationInput.Y * lookSpeedVertical * (invertCameraVertical ? -1 : 1), Mathf.DegToRad(-85), Mathf.DegToRad(85)),
            -rotationInput.X * lookSpeedHorizontal
        );
        
        // Left/right movements rotate the player base.
        RotateY(lookRotation.Y);
        // Up/down movements rotate the player head.
        head.RotateX(lookRotation.X);
        
        head.Rotation = new Vector3(float.Clamp(head.Rotation.X, MIN_VERTICAL_ANGLE_RADIANS, MAX_VERTICAL_ANGLE_RADIANS), 0, 0);

        if (lookRotation.LengthSquared() > 0)
            EmitSignalOnPlayerLook();
    }

    private void SetMouseCaptured(bool captured)
    {
        Input.SetMouseMode(captured
            ? Input.MouseModeEnum.Captured
            : Input.MouseModeEnum.Visible);
        mouseCaptured = captured;
    }
    
    /// <summary>
    /// Check if any expected Input Actions haven't been mapped.
    /// Throws exceptions if so.
    /// </summary>
    private void CheckInputMappings()
    {
        if (!InputMap.HasAction(inputName_MoveLeft))
            throw new Exception("Missing input map for left movement.");
        if (!InputMap.HasAction(inputName_MoveRight))
            throw new Exception("Missing input map for right movement.");
        if (!InputMap.HasAction(inputName_MoveForward))
            throw new Exception("Missing input map for forward movement.");
        if (!InputMap.HasAction(inputName_MoveBack))
            throw new Exception("Missing input map for back movement.");
    }
}