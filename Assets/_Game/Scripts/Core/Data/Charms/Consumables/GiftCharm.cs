using UnityEngine;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Data; // For ResourceType enum

[CreateAssetMenu(menuName = "Charms/Consumables/Ticket Bundle")]
public class TicketBundleCharm : ConsumableCharm
{
    [Header("Settings")]
    public int ticketAmount = 2;

    private void OnEnable()
    {
        consumeType = ConsumeType.Immediate;
        autoDiscard = true;
    }

    protected override void ActivateEffect()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddResource(ResourceType.Ticket, ticketAmount);
            Debug.Log($"[Ticket Bundle] Redeemed! +{ticketAmount} Tickets.");
        }
    }
}