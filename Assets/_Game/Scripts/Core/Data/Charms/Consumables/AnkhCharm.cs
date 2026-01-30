using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory; // <--- Add this namespace

[CreateAssetMenu(menuName = "Charms/Consumables/Ankh")]
public class AnkhCharm : ConsumableCharm
{
    [System.NonSerialized] private CharmHolder _myHolder;

    public override void OnEquip(CharmHolder holder)
    {
        _myHolder = holder;
    }

    public override bool OnPaymentCheck(int currentCoin, int currentDebt)
    {
        if (currentCoin < currentDebt)
        {
            Debug.Log($"[Ankh] Activated! Debt {currentDebt} not met. Granting extra rounds.");

            // 1. Discard the charm (It's a one-time use)
            if (_myHolder != null)
            {
                _myHolder.RemoveCharm(this);
            }
            else
            {
                Debug.LogWarning("[Ankh] No Holder found, cannot discard!");
            }

            // 2. Return TRUE to tell GameManager "Don't Game Over, I handled it."
            return true; 
        }
        return false; 
    }
}
