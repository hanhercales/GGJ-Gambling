using UnityEngine;
using _Game.Scripts.Core.Data;

[CreateAssetMenu(menuName = "Charms/Consumables/Ankh")]
public class AnkhCharm : ConsumableCharm
{
    private void OnEnable()
    {
        consumeType = ConsumeType.Delayed;
        autoDiscard = true;
    }

    public override bool OnPaymentCheck(int currentCoin, int currentDebt)
    {
        if (currentCoin < currentDebt)
        {
            return true; 
        }
        
        return false; 
    }
}