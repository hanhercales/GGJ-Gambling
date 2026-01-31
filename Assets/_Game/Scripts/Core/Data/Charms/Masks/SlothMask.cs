using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Managers;

[CreateAssetMenu(menuName = "Charms/Masks/Mask of Sloth")]
public class MaskOfSlothCharm : CharmData
{
    [Header("Settings")]
    public int spinsReduction = 2; 
    public int ticketReward = 1;   
    
    public override void OnEquip(CharmHolder holder)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ModifyGlobalSpinCount(-spinsReduction);
    }

    public override void OnUnequip(CharmHolder holder)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ModifyGlobalSpinCount(spinsReduction);
    }
    
    public override void OnRoundStart(GameManager gm)
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddResource(ResourceType.Ticket, ticketReward);
            Debug.Log($"[Mask of Sloth] Pack Finished. Lazy Bonus: +{ticketReward} Ticket.");
        }
    }
}