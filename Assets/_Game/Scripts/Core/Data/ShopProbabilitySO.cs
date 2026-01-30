using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [System.Serializable]
    public struct TierWeight
    {
        public CharmTier tier;
        public int weight; // Trọng số (VD: Common=100, Legendary=5)
    }

    [CreateAssetMenu(fileName = "NewShopProbability", menuName = "GameConfig/Shop Probability Profile")]
    public class ShopProbabilitySO : ScriptableObject
    {
        [Header("Tier Weights")]
        // List này định nghĩa tỉ lệ ra từng tier
        public List<TierWeight> tierWeights = new List<TierWeight>();

        // Hàm tiện ích để ShopManager lấy trọng số
        public int GetWeightForTier(CharmTier tier)
        {
            foreach (var tw in tierWeights)
            {
                if (tw.tier == tier) return tw.weight;
            }
            return 0;
        }
    }
}