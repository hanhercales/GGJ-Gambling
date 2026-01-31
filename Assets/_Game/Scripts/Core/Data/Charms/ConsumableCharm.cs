using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;

[CreateAssetMenu(menuName = "Charms/Base/Consumable Base")]
public class ConsumableCharm : CharmData
{
    public enum ConsumeType
    {
        Immediate,
        Delayed
    }

    [Header("Consumable Config")]
    public ConsumeType consumeType;
    [Tooltip("If true, automatically deletes itself after activation.")]
    public bool autoDiscard = true;
    
    public override void OnEquip(CharmHolder holder)
    {
        if (consumeType == ConsumeType.Immediate)
        {
            ActivateEffect();
            
            if (autoDiscard)
            {
                holder.RemoveCharm(this);
                Debug.Log($"[Consumable] {charmName} used and discarded instantly.");
            }
        }
        else
        {

            base.OnEquip(holder); 
        }
    }
    
    protected virtual void ActivateEffect() { }
}

