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

    // Runtime state
    [System.NonSerialized] private int _roundsRemaining = 0;
    [System.NonSerialized] private CharmHolder _myHolder;

    private void OnEnable() => _roundsRemaining = 0;
    
    public override void OnEquip(CharmHolder holder)
    {
        _myHolder = holder;
        _roundsRemaining = duration; // Set to 2
        Debug.Log($"[Lightbulb] Equipped! Active for next {duration} spins.");
    }

    public override void OnSpinStart(SlotMachineController machine, LuckManager luckManager)
    {
        if (_roundsRemaining > 0)
        {
            luckManager.baseLuckFromCharms += luckAmount;
            Debug.Log($"[Lightbulb] Active! +{luckAmount} Luck ({_roundsRemaining} spins left).");
        }
    }
    
    public override void OnSpinEnd(SlotMachineController machine, LuckManager luckManager)
    {
        // Cleanup Logic
        if (_roundsRemaining > 0)
        {
            // Remove the temporary luck
            luckManager.baseLuckFromCharms -= luckAmount;
            
            // Tick down
            _roundsRemaining--;

            // Discard Check
            if (_roundsRemaining <= 0)
            {
                Debug.Log("[Lightbulb] Burnt out! Discarding charm.");
                if (_myHolder != null)
                {
                    _myHolder.RemoveCharm(this);
                }
            }
        }
    }
}