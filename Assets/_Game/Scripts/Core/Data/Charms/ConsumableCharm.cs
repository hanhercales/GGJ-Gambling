using UnityEngine;
using _Game.Scripts.Core.Data;

[CreateAssetMenu(menuName = "Charms/Consumable")]
public class ConsumableCharm : CharmData
{
    public enum TriggerCondition
    {
        OnDebtPayment,
        OnDeath,
        OnShopEnter
    }

    public TriggerCondition condition;
    public bool destroyOnUse = true;

    // public override void OnSpinEnd()
    // {
    //     // if (condition == TriggerCondition.OnDeath && PlayerStats.IsDead)
    //     // {
    //     //     PlayerStats.Revive();
    //     //     if (destroyOnUse) PlayerInventory.RemoveCharm(this);
    //     // }
    // }
}

