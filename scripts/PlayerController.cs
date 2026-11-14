using System;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using MidnightBaking.scripts.interactables;

namespace MidnightBaking.scripts;

/// <summary>
/// Handles player first person camera and movement.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    public static PlayerController instance { get; private set; }

    private bool enabled = false;
    public bool canMove = true;
    public bool canLook = true;

    [Export] public Node3D heldObjectPointCentre;
    [Export] public Node3D heldObjectPointLeftHand;
    [Export] public Node3D heldObjectPointRightHand;
    [Export] private Control crosshair;

    [ExportGroup("Speeds")]
    [Export] private float lookSpeedHorizontal = 0.002f;
    [Export] private float lookSpeedVertical = 0.002f;
    [Export] private bool invertCameraVertical = false;
    [Export] private float moveSpeed = 3.0f;

    [ExportGroup("Input Names")]
    [Export] private string inputName_MoveLeft = "move_left";
    [Export] private string inputName_MoveRight = "move_right";
    [Export] private string inputName_MoveForward = "move_forward";
    [Export] private string inputName_MoveBack = "move_backward";
    
    [Signal] public delegate void OnPlayerLookEventHandler();
    [Signal] public delegate void OnPlayerMoveEventHandler();

    private Vector2 lookRotation;
    private float currentMoveSpeed;

    public Node3D head;
    private CollisionShape3D collider;

    private readonly float MIN_VERTICAL_ANGLE_RADIANS = float.DegreesToRadians(-85);
    private readonly float MAX_VERTICAL_ANGLE_RADIANS = float.DegreesToRadians(85);
    
    public override void _Ready()
    {
        if (instance != null)
            throw new Exception("Duplicate PlayerController instance. Should be exactly 1.");
        instance = this;
        
        head = GetNode<Node3D>("Head");
        collider = GetNode<CollisionShape3D>("Collider");
        CheckInputMappings();
        lookRotation.Y = Rotation.Y;
        lookRotation.X = head.Rotation.X;
        
        SetEnabled(false);
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        crosshair.Visible = enabled;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!enabled || !canLook) return;
        
        // Look around.
        if (@event is InputEventMouseMotion motionEvent)
        {
            RotateLook(motionEvent.Relative);
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!enabled) return;
        
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

//        Debug.Print($"Player pos {Position}, rot ({Mathf.RadToDeg(this.Quaternion.GetEuler().X)}, {Mathf.RadToDeg(head.Quaternion.GetEuler().Y)}, 0)");
        
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

    public void Save(Dictionary data)
    {
        data["position"] = Position;
        Debug.Print($"player position data type: {data["position"].VariantType}");
        data["rotation"] = new Vector2(head.Rotation.X, Rotation.Y);
        data["right_hand"] = (int) (Player.heldItemRight?.id ?? 0);
        data["left_hand"] = (int) (Player.heldItemLeft?.id ?? 0);
    }

    public void Load(Dictionary data)
    {
        Position = Utils.ParseVector3(data["position"]);
        Vector2 rotation = Utils.ParseVector2(data["rotation"]);
        head.SetRotation(new Vector3(rotation.X, 0, 0));
        SetRotation(new Vector3(0, rotation.Y, 0));
        
        // TODO unload any currently held items 
        
        var rightItemId = (ItemId) data["right_hand"].AsInt32();
        var leftItemId = (ItemId) data["left_hand"].AsInt32();
        if (rightItemId != ItemId.UNDEFINED)
        {
            var rightItem = Items.All[rightItemId] as Pickupable;
            Player.PickUpItem(rightItem);
        }
        if (leftItemId != ItemId.UNDEFINED && leftItemId != rightItemId)
        {
            var leftItem = Items.All[leftItemId] as Pickupable;
            Player.PickUpItem(leftItem);
        }
    }
}