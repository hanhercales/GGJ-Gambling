using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Voucher")]
public class VoucherCharm : CharmData
{
    [Header("Settings")]
    public int discountAmount = 1;

    public override void OnEquip(CharmHolder holder)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ModifyDiscount(discountAmount);
            Debug.Log($"[Coupon] Prices reduced by {discountAmount}!");
        }
    }

    public override void OnUnequip(CharmHolder holder)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ModifyDiscount(-discountAmount);
            Debug.Log($"[Coupon] Discount removed.");
        }
    }
}
