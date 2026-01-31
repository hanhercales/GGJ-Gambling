using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Inventory;

[CreateAssetMenu(menuName = "Charms/Effects/Slippers")]
public class FragileClutchCharm : CharmData
{
    [Header("Clutch Settings")]
    public int requiredLosses = 3;
    public int luckBonus = 9;
    
    [Header("Risk Settings")]
    [Range(0, 100)] public int breakChance = 36;

    // Runtime State
    [System.NonSerialized] private int _lossStreak = 0;
    [System.NonSerialized] private bool _bonusActive = false;
    [System.NonSerialized] private CharmHolder _myHolder;

    private void OnEnable()
    {
        _lossStreak = 0;
        _bonusActive = false;
    }

    public override void OnEquip(CharmHolder holder)
    {
        _myHolder = holder;
        _lossStreak = 0;
    }

    public override void OnSpinStart(SlotMachineController machine, LuckManager luckManager)
    {
        if (_lossStreak >= requiredLosses)
        {
            luckManager.baseLuckFromCharms += luckBonus;
            _bonusActive = true;
            Debug.Log($"[Fragile Clutch] Loss Streak {_lossStreak}! +{luckBonus} Luck activated.");
        }
    }
    
    public override void OnSpinResult(SlotMachineController machine, LuckManager luckManager, float winAmount)
    {
        if (_bonusActive)
        {
            luckManager.baseLuckFromCharms -= luckBonus;
            _bonusActive = false;
            _lossStreak = 0;
            
            int roll = Random.Range(0, 100);
            
            if (roll < breakChance)
            {
                Debug.Log($"[Fragile Clutch] The charm shattered! (Rolled {roll} < {breakChance})");
                if (_myHolder != null)
                {
                    _myHolder.RemoveCharm(this);
                }
            }
            else
            {
                Debug.Log($"[Fragile Clutch] The charm survived. (Rolled {roll})");
            }
            return; 
        }
        
        if (winAmount > 0)
        {
            _lossStreak = 0;
        }
        else
        {
            _lossStreak++;
            Debug.Log($"[Fragile Clutch] Streak increased: {_lossStreak}");
        }
    }
}
