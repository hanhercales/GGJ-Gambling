using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Family/Uncommon Family")]
public class USymbolCharm : CharmData
{
    [Header("Settings")]
    public SymbolData targetedSymbol;
    
    [Tooltip("How many 'Levels' to add? (Description says +2, so put 2 here)")]
    public int bonusLevels = 2; 

    // 1. When you buy/pick up the charm: Apply the Buff
    public override void OnEquip(CharmHolder holder)
    {
        if (targetedSymbol != null && WeightManager.Instance != null)
        {
            // Logic: Calls WeightManager -> SymbolData.AddWeightLevel(2)
            // Result: Adds 1.6 to the weight (Internally +16)
            WeightManager.Instance.ApplyBuffLevel(targetedSymbol, bonusLevels);
            
            Debug.Log($"[USymbolCharm] Equipped! {targetedSymbol.idName} weight +{bonusLevels} Levels.");
        }
    }

    // 2. When you sell/lose the charm: Remove the Buff
    public override void OnUnequip(CharmHolder holder)
    {
        if (targetedSymbol != null && WeightManager.Instance != null)
        {
            // Logic: Subtract the levels to return to normal
            WeightManager.Instance.ApplyBuffLevel(targetedSymbol, -bonusLevels);
            
            Debug.Log($"[USymbolCharm] Unequipped! {targetedSymbol.idName} weight restored.");
        }
    }
}