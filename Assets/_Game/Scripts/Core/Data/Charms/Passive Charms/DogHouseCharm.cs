using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;

[CreateAssetMenu(menuName = "Charms/Consumables/Dog House (Slot Upgrade)")]
public class DogHouseCharm : ConsumableCharm
{
    [Header("Upgrade Settings")]
    public int extraSlots = 1;

    private void OnEnable()
    {
        consumeType = ConsumeType.Immediate;
        autoDiscard = true; 
        oneTimePurchase = true; 
    }

    public override void OnEquip(CharmHolder holder)
    {
        if (holder != null)
        {
            holder.ModifyCapacity(extraSlots);
            Debug.Log($"[Dog House] Renovations complete! Inventory size increased by {extraSlots}.");
        }
        
        base.OnEquip(holder);
    }

    protected override void ActivateEffect() { }
}