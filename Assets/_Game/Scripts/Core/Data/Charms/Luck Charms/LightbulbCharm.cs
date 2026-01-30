using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Inventory;

[CreateAssetMenu(menuName = "Charms/Effects/Lightbulb")]
public class LightbulbCharm : CharmData
{
    [Header("Lightbulb Settings")]
    public int luckAmount = 15;
    public int duration = 2;

    [System.NonSerialized] private int _roundsRemaining = 0;
    [System.NonSerialized] private CharmHolder _myHolder;

    private void OnEnable() => _roundsRemaining = 0;
    
    public override void OnEquip(CharmHolder holder)
    {
        _myHolder = holder;
        _roundsRemaining = duration; 
        Debug.Log($"[Lightbulb] Equipped! Active for {duration} spins.");
    }

    public override void OnSpinStart(SlotMachineController machine, LuckManager luckManager)
    {
        // Chỉ buff nếu còn lượt
        if (_roundsRemaining > 0)
        {
            luckManager.baseLuckFromCharms += luckAmount;
            Debug.Log($"[Lightbulb] Active! +{luckAmount} Luck ({_roundsRemaining} left).");
        }
    }
    
    public override void OnSpinEnd(SlotMachineController machine, LuckManager luckManager)
    {
        if (_roundsRemaining > 0)
        {
            // 1. Trả lại Luck
            luckManager.baseLuckFromCharms -= luckAmount;
            
            // 2. Trừ lượt tồn tại
            _roundsRemaining--;

            // 3. Kiểm tra hủy
            if (_roundsRemaining <= 0)
            {
                Debug.Log("[Lightbulb] Burnt out! Removing...");
                if (_myHolder != null)
                {
                    _myHolder.RemoveCharm(this);
                }
            }
        }
    }
}