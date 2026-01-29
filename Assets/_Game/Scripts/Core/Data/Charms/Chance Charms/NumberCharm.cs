using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using UnityEngine;
using _Game.Scripts.Core.Inventory;

[CreateAssetMenu(menuName = "Charms/Chance/Funny Number")]
public class NumberCharm : ChanceCharm
{
    [Header("Settings")]
    public int luckBonus = 6;
    public int maxUses = 9;
    
    [System.NonSerialized] private int _currentUses = 0;
    [System.NonSerialized] private bool _isActiveThisTurn = false;
    [System.NonSerialized] private CharmHolder _myHolder;
    
    public override void OnEquip(CharmHolder holder)
    {
        _myHolder = holder;
        _currentUses = 0; // Reset counter on purchase
    }

    public override void OnSpinStart(SlotMachineController machine)
    {
        _isActiveThisTurn = false;

        // Roll the dice
        if (TryTrigger())
        {
            // Apply Buff
            machine.currentLuck += luckBonus;
                
            // Mark state
            _isActiveThisTurn = true;
            _currentUses++;
                
            Debug.Log($"[Charm] Triggered! ({_currentUses}/{maxUses} uses)");
        }
    }

    public override void OnSpinEnd(SlotMachineController machine)
    {
        if (_isActiveThisTurn)
        {
            machine.currentLuck -= luckBonus;
            _isActiveThisTurn = false;
            
            if (_currentUses >= maxUses)
            {
                Debug.Log("[Charm] finished its mission! Discarding...");
                if (_myHolder != null)
                {
                    _myHolder.RemoveCharm(this);
                }
            }
        }
    }
}
