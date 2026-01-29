using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using UnityEngine;
using _Game.Scripts.Core.Inventory;


[CreateAssetMenu(menuName = "Charms/Effects/Clutch (Loss Streak)")]
public class ConsoPrizeCharm : CharmData
{
    [Header("Settings")]
    public int requiredLosses = 2; // "Last two spins"
    public int luckBonus = 7;

    [System.NonSerialized] private int _lossStreak = 0;
    [System.NonSerialized] private bool _bonusActive = false;

    private void OnEnable()
    {
        _lossStreak = 0;
        _bonusActive = false;
    }

    public override void OnSpinStart(SlotMachineController machine)
    {
        if (_lossStreak >= requiredLosses)
        {
            machine.currentLuck += luckBonus;
            _bonusActive = true;
            Debug.Log($"[Clutch] Loss Streak {_lossStreak}! Activating +{luckBonus} Luck.");
        }
    }
    
    public override void OnSpinResult(SlotMachineController machine, float winAmount)
    {
        // Clean after eating
        if (_bonusActive)
        {
            machine.currentLuck -= luckBonus;
            _bonusActive = false;
            _lossStreak = 0; 
            return; 
        }

        // Reset streak
        if (winAmount > 0)
        {
            _lossStreak = 0;
        }
        else
        {
            _lossStreak++;
            Debug.Log($"[Clutch] Lost spin. Current streak: {_lossStreak}");
        }
    }
}
