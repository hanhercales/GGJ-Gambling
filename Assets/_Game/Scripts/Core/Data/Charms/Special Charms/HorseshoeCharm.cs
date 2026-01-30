using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Passives/Horseshoe")]
public class HorseshoeCharm : CharmData
{
    public override void OnEquip(CharmHolder holder)
    {
        // "I am here!"
        if (LuckManager.Instance != null)
            LuckManager.Instance.UpdateHorseshoeCount(1);
    }

    public override void OnUnequip(CharmHolder holder)
    {
        // "I am leaving!"
        if (LuckManager.Instance != null)
            LuckManager.Instance.UpdateHorseshoeCount(-1);
    }
}