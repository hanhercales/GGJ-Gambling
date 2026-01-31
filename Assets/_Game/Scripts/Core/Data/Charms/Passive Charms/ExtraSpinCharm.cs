using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Extra Spins")]
public class ExtraSpinCharm : CharmData
{
    [Header("Settings")]
    public int extraSpins = 2;

    // 1. Trường hợp: Đã sở hữu từ trước -> Kích hoạt khi bắt đầu vòng mới
    public override void OnRoundStart(GameManager gameManager)
    {
        gameManager.AddSpins(extraSpins);
        Debug.Log($"[ExtraSpin] New Round Bonus: +{extraSpins} spins.");
    }

    // 2. Trường hợp: Mới mua trong Shop -> Kích hoạt NGAY LẬP TỨC
    public override void OnEquip(CharmHolder holder)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddSpins(extraSpins);
            Debug.Log($"[ExtraSpin] Just Bought: +{extraSpins} spins applied immediately.");
        }
    }

    // 3. Trường hợp: Bán đi -> Phải trừ lại số spin đã cộng
    public override void OnUnequip(CharmHolder holder)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddSpins(-extraSpins);
            Debug.Log($"[ExtraSpin] Sold: Removed {extraSpins} spins.");
        }
    }
}