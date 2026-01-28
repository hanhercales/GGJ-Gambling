using _Game.Scripts.Core.Data;
using UnityEngine;

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
        public int price;
        public bool isEquipped;
        
        [TextArea] public string description;
        public CharmData[] requiredCharmToUnlock;

        public virtual void OnSpinStart()
        {
            
        }
        
        public virtual void OnSpinEnd()
        {
            
        }
        
        public virtual bool OnBoardGenerated(string[,] boardIds)
        {
            return false;
        }

        public virtual long ModifySymbolScore(long currentScore, SymbolData symbolId, string symbolName)
        {
            return currentScore;
        }
        
    }
}
