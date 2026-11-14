using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;

namespace MidnightBaking.scripts;

/// <summary>
/// A physical object in the scene which the player can interact with.
/// <br/><br/>The object's name <b>must</b> uniquely match with a corresponding <see cref="ItemId"/>.
/// Case, spaces, and underscores are ignored when checking for a match, e.g. "Foo 1", "foo1", and "FOO_1"
/// are interchangeable.
/// </summary>
public partial class Interactable : Node3D, Resetable
{
    /// <summary>
    /// DEBUG ONLY. List of interactable object names (must be exact) to log events for.
    /// </summary>
    private static string[] DEBUG_ITEMS_TO_LOG = [];
    public enum Requirement { NO_REQUIREMENT, REQUIRES_ONE_HAND, REQUIRES_BOTH_HANDS }
    
    public ItemId id { get; private set; }
    /// <summary>Meshes to highlight when this object's interactions are enabled.</summary>
    [Export] public Array<MeshInstance3D> highlightMeshes;
    /// <summary>CSGs to highlight when this object's interactions are enabled.</summary>
    [Export] public Array<CsgPrimitive3D> highlightCsgs;
    /// <summary>Default player status required to interact with this object. Individual <see cref="InteractionRequest"/>s can optionally specify their own requirement.</summary>
    [Export] public Requirement requirement;
    
    protected bool interactionsOn = true;
    protected readonly List<InteractionRequest> interactionRequests = [];

    /// <summary>
    /// Details about a registered interaction. Note that <see cref="highlight"/> and <see cref="highlightCheck"/>
    /// are mutually exclusive; the highlightCheck method is only used if no specific true or false value is
    /// supplied for highlight.
    /// </summary>
    protected class InteractionRequest(Action onClickCallback, bool? highlightValue, Func<bool> highlightCheck, Requirement? requirementOverride, bool autoUnsubscribe)
    {
        public readonly Action onClickCallback = onClickCallback;
        public bool? highlightValue = highlightValue;
        public readonly Func<bool> highlightCheck = highlightCheck;
        public bool highlight => highlightValue ?? highlightCheck.Invoke();
        public Requirement? requirementOverride = requirementOverride;
        public bool autoUnsubscribe = autoUnsubscribe;
    }
    
    public override void _Ready()
    {
        id = Items.RegisterInteractable(this);
        ResetToGameStartState();
    }
    
    public void ResetToGameStartState() => _ResetToGameStartState();
    
    /// <summary>
    /// Optional hook method for subclasses to override. Invoked during <see cref="ResetToGameStartState()"/>.
    /// </summary>
    protected virtual void _ResetToGameStartState() { }

    /// <summary>
    /// Set whether the user can interact with this object.
    /// </summary>
    public void SetCanInteract(bool canInteract)
    {
        interactionsOn = canInteract;
        UpdateHighlights();
    }
    
    // TODO can we have click listeners auto unsubscribe on click, by default? can pass an optional argument which
    // tells it *not* to unsubscribe on click.
    /// <summary>
    /// Register a callback to be invoked when this Interactable is clicked. Highlights the interactable by default,
    /// though this can be toggled off. Note that the Interactable may still be highlighted even if the highlight is
    /// turned off, as there may be other listeners which have turned it on.
    /// </summary>
    /// <param name="callback">Invoked on click, if the interaction requirements are currently met.</param>
    /// <param name="highlight">Method which checks if the highlight should be enabled. This is only used during <see cref="UpdateHighlights()"/>, though you can invoke UpdateHighlights manually outside of its normal uses. Note that the highlight will be on while <i>any</i> listener has it turned on.</param>
    /// <param name="interactionRequirement">Optionally specifies a minimum player state requirement in order to trigger this callback. If no value is specified, uses this Interactable's default <see cref="requirement"/> property.</param>
    /// <param name="autoUnsubscribe">If true, automatically remove this click listener when it is first invoked.</param>
    /// <param name="debug_callbackName">Optionally provide a name or description of the callback, for debugging purposes.<br/><br/>Should be provided by default; it's only optional for backwards compatibility.</param>
    public void AddClickListener(
        Action callback,
        Func<bool> highlight,
        Requirement? interactionRequirement = null,
        bool autoUnsubscribe = true,
        string debug_callbackName = null)
    => AddClickListener(callback, null, highlight, interactionRequirement, autoUnsubscribe, debug_callbackName);

    /// <summary>
    /// Register a callback to be invoked when this Interactable is clicked. Highlights the interactable by default,
    /// though this can be toggled off. Note that the Interactable may still be highlighted even if the highlight is
    /// turned off, as there may be other listeners which have turned it on.
    /// </summary>
    /// <param name="callback">Invoked on click, if the interaction requirements are currently met.</param>
    /// <param name="highlight">Turn the object's highlight on? Note that the highlight will be on while <i>any</i> listener has it turned on.</param>
    /// <param name="autoUnsubscribe">If true, automatically remove this click listener when it is first invoked.</param>
    /// <param name="interactionRequirement">Optionally specifies a minimum player state requirement in order to trigger this callback. If no value is specified, uses this Interactable's default <see cref="requirement"/> property.</param>
    /// <param name="debug_callbackName">Optionally provide a name or description of the callback, for debugging purposes.<br/><br/>Should be provided by default; it's only optional for backwards compatibility.</param>
    public void AddClickListener(
        Action callback,
        bool highlight = true,
        Requirement? interactionRequirement = null,
        bool autoUnsubscribe = true,
        string debug_callbackName = null)
    => AddClickListener(callback, highlight, null, interactionRequirement, autoUnsubscribe, debug_callbackName);
    
    /// <summary>
    /// See the public AddClickListener() methods for description.
    /// <br/>Note: <paramref name="highlight"/> and <paramref name="highlightPredicate"/> are mutually exclusive; exactly
    /// one must have a value, while the other must be null.
    /// </summary>
    private void AddClickListener(
        Action callback,
        bool? highlight,
        Func<bool> highlightPredicate,
        Requirement? interactionRequirement,
        bool autoUnsubscribe,
        string debug_callbackName)
    {
        foreach (string itemToLog in DEBUG_ITEMS_TO_LOG)
            if (itemToLog == Name)
                Debug.Print($"{itemToLog}.AddClickListener({(debug_callbackName == null ? "" : $"callback: {debug_callbackName}, ")}highlight ? {(highlight.HasValue ? highlight.Value : "conditional")}, requirementOverride ? {(interactionRequirement == null ? "null" : interactionRequirement.ToString())}) - existing requests {interactionRequests.Count}");
        
        if (interactionRequests.Any(it => it.onClickCallback == callback))
            throw new Exception($"Attempted to add a duplicate onClick callback to Interactable \"{Name}\". Callbacks count = {interactionRequests.Count}.");

        if (interactionRequests.Count == 0)
            Player.RegisterItemHoldChangedListener(UpdateHighlights);
        interactionRequests.Add(new InteractionRequest(callback, highlight, highlightPredicate, interactionRequirement, autoUnsubscribe));
        UpdateHighlights();
    }

    public void RemoveClickListener(Action callback)
    {
        foreach (string itemToLog in DEBUG_ITEMS_TO_LOG)
            if (itemToLog == Name)
                Debug.Print($"{itemToLog}.RemoveClickListener() - listener exists ? {interactionRequests.Any(it => it.onClickCallback == callback)}, existing requests {interactionRequests.Count}");
        
        // Due to being able to optionally skip steps, it's important to be able to 
        // ATTEMPT to remove callbacks even if they aren't there. So calling
        // Remove() even when a callback hasn't been Added is a completely valid case.
        var request = interactionRequests.FirstOrDefault(it => it.onClickCallback == callback);
        if (request == null)
            return;
        
        interactionRequests.Remove(request);
        if (interactionRequests.Count == 0)
            Player.RemoveItemHoldChangedListener(UpdateHighlights);
        UpdateHighlights();
    }
    
    public void UpdateHighlights()
    {
        var minimumInteractionRequirement = Requirement.NO_REQUIREMENT;
        if (interactionRequests.Count > 0)
            minimumInteractionRequirement = interactionRequests.Min(it => it.requirementOverride ?? requirement);

        Material material = interactionsOn
                            && interactionRequests.Any(it => it.highlight)
                            && Player.CanInteract(minimumInteractionRequirement)
            ? Game.flashMaterialOverlay
            : null;
        if (Name == "Flour Jar")
            Debug.Print($"Flour Jar.UpdateHighlights() - interactions on ? {interactionsOn}, interaction requests ? {interactionRequests.Count(it => it.highlight)}, requirement: {minimumInteractionRequirement}, can interact ? {Player.CanInteract(minimumInteractionRequirement)}");
        
        foreach (string itemToLog in DEBUG_ITEMS_TO_LOG)
            if (itemToLog == Name)
                Debug.Print($"{itemToLog}.UpdateHighlights() - interactions on {interactionsOn}, requests {interactionRequests.Count}, any requests on ? {interactionRequests.Any(it => it.highlight)}, min requirement {minimumInteractionRequirement}, player can interact ? {Player.CanInteract(minimumInteractionRequirement)}, highlight ? {material != null}");

        foreach (var mesh in highlightMeshes)
            mesh.MaterialOverlay = material;
        foreach (var csg in highlightCsgs)
            csg.MaterialOverlay = material;
    }

    /// <summary>
    /// Sets this Interactable as a child of the specified parent, and resets its position and rotation to 0
    /// relative to the new parent.
    /// </summary>
    public virtual void SetPlacement(Node3D newParent)
    {
        // TODO figure out how to handle this during scene setup. A node's parent is readied AFTER its children.
        if (GetParent() == null || newParent == null)
        {
            GD.Print($"{Name}.Interactable tried to call SetPlacement() but the {(newParent == null ? "new" : "old")} parent was null.");
            return;
        }

        // Using Reparent() during scene setup (and possibly other times) causes errors.
        // So we use CallDeferred() to indirectly invoke Reparent() on the next free frame.
        // This ensures Reparent() is only called once everything is set up.
        CallDeferred("ReparentDeferred", newParent);
    }

    /// <summary>
    /// Companion method to <see cref="SetPlacement()"/>. Should not be invoked by any other method.
    /// </summary>
    private void ReparentDeferred(Node3D newParent)
    {
        Reparent(newParent);
        Position = Vector3.Zero;
        RotationDegrees = Vector3.Zero;
        if (Player.IsHolding(this))
            Player.StopHoldingItem(this);
    }

    /// <summary>
    /// Should be invoked by this object's associated collider (often a child),
    /// via its attached <see cref="InteractionTarget"/>.
    /// </summary>
    public void OnInteractableClicked()
    {
        if (!interactionsOn) return;
        _HandleClick();
        // Iterate backwards in case the current item removes itself from the list.
        for (int i = interactionRequests.Count - 1; i >= 0; i--)
        {
            var interaction = interactionRequests[i];
            if (Player.CanInteract(interaction.requirementOverride ?? requirement))
            {
                if (interaction.autoUnsubscribe)
                    RemoveClickListener(interaction.onClickCallback);
                interaction.onClickCallback.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Optional hook method for subclasses to override. Invoked during <see cref="OnInteractableClicked()"/>.
    /// </summary>
    protected virtual void _HandleClick() {}

    /// <summary>
    /// Set this Interactable's state based on a game state stored as json.
    /// </summary>
    public virtual void Load(Dictionary data) { }

    /// <summary>
    /// Save this Interactable's state to json.
    /// </summary>
    public virtual void Save(Dictionary data) { }
}
