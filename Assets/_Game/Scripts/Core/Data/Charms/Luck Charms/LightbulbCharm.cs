using System;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Effects/Lightbulb")]
public class LightbulbCharm : CharmData
{
    [Header("Lightbulb Settings")]
    public int luckAmount = 15;
    public int duration = 2;

    [System.NonSerialized] private int _roundsRemaining = 0;

    private void OnEnable() => _roundsRemaining = 0;

    public override void OnSpinStart(SlotMachineController machine, LuckManager luckManager)
    {
        _roundsRemaining = duration;
        Debug.Log($"[Lightbulb] Triggered! Buff active for next {duration} spins.");

        if (_roundsRemaining > 0)
        {
            luckManager.baseLuckFromCharms += luckAmount;
        }
    }
    
    public override void OnSpinEnd(SlotMachineController machine, LuckManager luckManager)
    {
        // 3. Clean up: Remove the luck we added so it doesn't stack forever
        if (_roundsRemaining > 0)
        {
            luckManager.baseLuckFromCharms -= luckAmount;
                
            // 4. Tick down the timer
            _roundsRemaining--;
                
            Debug.Log($"[Lightbulb] Buff tick. Remaining: {_roundsRemaining}");
        }
    }
}
