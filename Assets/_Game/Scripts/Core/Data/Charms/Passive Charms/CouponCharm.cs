using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Coupon (Discount)")]
public class DiscountCharm : CharmData
{
    [Header("Settings")]
    public int discountAmount = 1;

    public override void OnEquip(CharmHolder holder)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ModifyDiscount(discountAmount);
            Debug.Log($"[Coupon] Equipped! Prices reduced by {discountAmount}.");
        }
    }

    public override void OnUnequip(CharmHolder holder)
    {
        if (ShopManager.Instance != null)
        {
            // Subtract the discount (return to normal)
            ShopManager.Instance.ModifyDiscount(-discountAmount);
            Debug.Log($"[Coupon] Unequipped! Prices restored.");
        }
    }
}