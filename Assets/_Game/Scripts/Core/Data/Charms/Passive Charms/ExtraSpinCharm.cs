using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Extra Spins")]
public class ExtraSpinCharm : CharmData
{
    [Header("Settings")]
    public int extraSpins = 2;
    
    public override void OnEquip(CharmHolder holder)
    {
        if (GameManager.Instance != null)
        {
            // Tăng chỉ số cộng thêm toàn cục
            GameManager.Instance.ModifyGlobalSpinCount(extraSpins);
            Debug.Log($"[ExtraSpin] Equipped: Global Spin Modifier +{extraSpins}");
        }
    }

    public override void OnUnequip(CharmHolder holder)
    {
        if (GameManager.Instance != null)
        {
            // Trừ lại khi tháo ra
            GameManager.Instance.ModifyGlobalSpinCount(-extraSpins);
            Debug.Log($"[ExtraSpin] Unequipped: Global Spin Modifier -{extraSpins}");
        }
    }
}