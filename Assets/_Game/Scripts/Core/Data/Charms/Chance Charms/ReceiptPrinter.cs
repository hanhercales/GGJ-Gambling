using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Managers;
using _Game.Scripts.Core.Logic;

[CreateAssetMenu(menuName = "Charms/Effects/Receipt Printer")]
public class ReceiptPrinter : ChanceCharm
{
    [Header("Reward Settings")]
    public int ticketAmount = 1;
    
    public override void OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results)
    {
        foreach (var match in results)
        {
            if (TryTrigger()) 
            {
                ResourceManager.Instance.AddResource(ResourceType.Ticket, ticketAmount);
                Debug.Log($"[TicketBonus] Match on {match.symbol.idName} triggered a Ticket!");
            }
        }
    }
}