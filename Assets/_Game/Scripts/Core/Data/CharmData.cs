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
        CharmTier tier;
    }
}

