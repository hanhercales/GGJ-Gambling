using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    public enum SpinOptionType
    {
        Standard,   // Gói thường (7 spins)
        FewerSpins, // Gói ít lượt (4 spins, nhiều vé hơn)
        Underspin,  // Gói thiếu tiền (Mua được bao nhiêu thì mua)
        Free        // Gói phá sản (1 spin miễn phí)
    }

    [System.Serializable]
    public class SpinOption
    {
        public SpinOptionType type;
        public string title;
        public string description; // Dùng cho Tooltip
        public int spinCount;
        public int coinCost;
        public int ticketReward;
        public bool isAffordable;
        public bool isAvailable;   // Nút này có được hiện/bấm không?
    }
}