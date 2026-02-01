using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Greed")]
public class GreedMask : MaskData // [THAY ĐỔI] Kế thừa MaskData
{
    [Header("Settings")]
    public float moneyMultiplier = 2f; // Double Money

    public override void OnEquip(CharmHolder holder)
    {
        // 1. Activate Greed (Double Money)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetGreedMultiplier(moneyMultiplier);
        }

        // 2. Activate Curse (No Tickets)
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.SetTicketBlock(true);
        }
        
        Debug.Log("[Mask of Greed] EQUIPPED: Money x2, Tickets BLOCKED.");
    }

    public override void OnUnequip(CharmHolder holder)
    {
        // 1. Deactivate Greed (Reset to 1x)
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetGreedMultiplier(1f);
        }

        // 2. Lift Curse (Allow Tickets)
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.SetTicketBlock(false);
        }

        Debug.Log("[Mask of Greed] UNEQUIPPED: Normal stats restored.");
    }
}