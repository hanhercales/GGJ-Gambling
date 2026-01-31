using UnityEngine;
using System.Numerics;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Consumables/Bailout Grant")]
public class GrantDebtCharm : ConsumableCharm
{
    [Header("Settings")]
    [Range(1, 100)] public int percentage = 40;

    // Constructor: Force settings for this specific item type
    private void OnEnable()
    {
        consumeType = ConsumeType.Immediate; // It's an instant potion
        autoDiscard = true;
    }

    protected override void ActivateEffect()
    {
        BigInteger currentDebt = ResourceManager.Instance.GetResourceBigInt(ResourceType.Debt);
        BigInteger bonusAmount = (currentDebt * percentage) / 100;

        if (bonusAmount > 0)
        {
            ResourceManager.Instance.AddResource(ResourceType.Coin, bonusAmount);
            Debug.Log($"[Bailout] Grant applied: +{bonusAmount} Coins");
        }
    }
}