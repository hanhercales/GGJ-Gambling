using _Game.Scripts.Core.Data;
using UnityEngine;
using _Game.Scripts.Core.Inventory;

[CreateAssetMenu(menuName = "Charms/Passives/Estate")]
public class EstateCharm : CharmData
{
    [Header("Settings")]
    public int extraSlots = 2;

    public override void OnEquip(CharmHolder holder)
    {
        holder.ModifyCapacity(extraSlots);
        Debug.Log($"[Estate] Expanded inventory by {extraSlots} slots.");
    }

    public override void OnUnequip(CharmHolder holder)
    {
        // IMPORTANT: Decrease capacity when sold/lost!
        holder.ModifyCapacity(-extraSlots);
        Debug.Log($"[Estate] Removed {extraSlots} slots.");
    }
}
