using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Gluttony")]
public class GluttonyMask : CharmData
{
    [Header("Settings")]
    public int freeSlotsCount = 2; // "2 Free Items"
    public float debtIncrease = 0.15f; // "+15% Debt"
    
    public override void OnEquip(CharmHolder holder)
    {
        if (DebtManager.Instance != null)
        {
            DebtManager.Instance.ModifyDebtMultiplier(debtIncrease);
        }
    }
    
    public override void OnUnequip(CharmHolder holder)
    {
        if (DebtManager.Instance != null)
        {
            DebtManager.Instance.ModifyDebtMultiplier(-debtIncrease);
        }
    }
    
    public override void OnShopRolled(ShopManager shop)
    {
        if (shop == null) return;
        
        List<CharmData> items = shop.GetCurrentItems();
        if (items.Count == 0) return;
        
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null) availableIndices.Add(i);
        }
        
        int picks = 0;
        while (picks < freeSlotsCount && availableIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int slotToDiscount = availableIndices[randomIndex];
            
            shop.SetSlotFree(slotToDiscount);
            
            availableIndices.RemoveAt(randomIndex);
            picks++;
        }
        
        Debug.Log($"[Mask of Gluttony] Made {picks} shop items FREE.");
    }
}