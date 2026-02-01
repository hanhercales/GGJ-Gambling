using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Lust")]
public class LustMask : CharmData
{
    [Header("Buff Settings")]
    public float rarityBoost = 0.3f; // +30% weight for high tiers

    [Header("Curse Settings")]
    public float priceMultiplier = 2.0f; // Double the cost

    public override void OnEquip(CharmHolder holder)
    {
        // 1. BUFF: Increase Rare & Legendary weights
        if (ShopManager.Instance != null)
        {
            // Adding 0.3 means 1.3x multiplier (30% increase)
            ShopManager.Instance.ModifyTierWeight(CharmTier.Rare, rarityBoost);
            ShopManager.Instance.ModifyTierWeight(CharmTier.Legendary, rarityBoost);
        }

        // 2. CURSE: Double Spin Price
        if (SpinManager.Instance != null)
        {
            SpinManager.Instance.SetCostMultiplier(priceMultiplier);
        }
        
        Debug.Log("[Mask of Lust] EQUIPPED: Better Shop, Expensive Spins.");
    }

    public override void OnUnequip(CharmHolder holder)
    {
        // 1. Remove Buff
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ModifyTierWeight(CharmTier.Rare, -rarityBoost);
            ShopManager.Instance.ModifyTierWeight(CharmTier.Legendary, -rarityBoost);
        }

        // 2. Remove Curse (Reset to 1.0)
        if (SpinManager.Instance != null)
        {
            SpinManager.Instance.SetCostMultiplier(1.0f);
        }
        
        Debug.Log("[Mask of Lust] UNEQUIPPED: Stats normalized.");
    }
}