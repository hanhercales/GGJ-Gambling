using System.Collections.Generic;
using _Game.Scripts.Controllers.Machines;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory;
using _Game.Scripts.Core.Logic;
using _Game.Scripts.Core.Managers;

using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.Scripts.Core.Data
{
    public enum CharmTier
    {
        Common,
        Uncommon,
        Rare,
        Legendary,
    }
    
    public class CharmData : RandomSOData
    {
        public CharmTier tier;
        public string charmName;
        public int price;
        public bool isEquipped;
        public bool oneTimePurchase = false;
        
        [TextArea] public string description;
        public CharmData[] requiredCharmToUnlock;

        public virtual void OnSpinStart(SlotMachineController machine, LuckManager luckManager) { }
        
        public virtual void OnSpinEnd(SlotMachineController machine, LuckManager luckManager) { }
        
        public virtual bool OnBoardGenerated(string[,] boardIds)
        {
            return false;
        }

        public virtual long ModifySymbolScore(long currentScore, SymbolData symbolId, string symbolName)
        {
            return currentScore;
        }

        public virtual bool OnPaymentCheck(int currentCoin, int currentDebt)
        {
            return false;
        }

        public virtual void OnDeadlineStart(GameManager gameManager) { }
        public virtual void OnEquip(CharmHolder holder) { }
        public virtual void OnUnequip(CharmHolder holder) { }
        public virtual void OnRoundStart(GameManager gameManager) { }

        public virtual void OnSpinResult(SlotMachineController machine, LuckManager luckManager, float winAmount) { }

        public virtual void OnSpinResultBuff(SlotMachineController machine, List<MatchResult> results) { }
        public virtual void OnShopRolled(ShopManager shop) { }
        
        public bool IsUnlockable(CharmHolder playerInventory)
        {
            if (requiredCharmToUnlock == null || requiredCharmToUnlock.Length == 0) 
                return true;

            foreach (var req in requiredCharmToUnlock)
            {
                // If we don't have the required parent, we can't buy this child
                if (!playerInventory.HasCharm(req)) 
                    return false;
            }
            return true;
        }
    }
}
