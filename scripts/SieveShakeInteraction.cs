using System;
using System.Diagnostics;
using Godot;

namespace MidnightBaking.scripts;

public partial class SieveShakeInteraction : Node3D
{
    /*
     * - Controls
     *      - Disable normal player controls
     *      - Map mouse movement to sieve movement
     * - Setup
     *      - Move camera into locked position
     *      - Move sieve to hover above the bowl
     * - UI
     *      - Show text "Gently shake the sieve by moving the mouse side to side"
     *      - Show mouse icon with arrows on either side, possibly with some alternating animation?
     * - Game Logic
     *      - Have the powder drain based on movement
     *      - Keep track of how much is spilled.
     * - Particles / Graphics
     *      - Show powder falling particles based on current powder drain rate
     *      - Show "splash" / "dust cloud" particles if moving too fast
     * -
     */

    public void Start(Action onCompleteCallback)
    {
        Debug.Print("STARTING SIEVE SHAKE GAME");
        Player.EnableNormalControls(false);
        
        
    }
}