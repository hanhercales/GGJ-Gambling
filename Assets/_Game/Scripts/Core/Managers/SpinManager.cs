using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory; // Để dùng ResourceType
using System.Numerics;

namespace _Game.Scripts.Core.Managers
{
    public class SpinManager : MonoBehaviour
    {
        public static SpinManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- CÔNG THỨC TÍNH GIÁ ---
        public int CalculateBaseSpinCost(int currentDebtRound)
        {
            // Công thức: Max(1, 2 * (Round - 1) * (1 + ((Round - 1) / 5)))
            // Round bắt đầu từ 1
            if (currentDebtRound <= 1) return 1; // Round 1 giá mặc định là 1 (hoặc theo công thức trả về 0 thì clamp lên 1)

            int term1 = 2 * (currentDebtRound - 1);
            int term2 = 1 + ((currentDebtRound - 1) / 5);
            int cost = Mathf.Max(1, term1 * term2);
            
            return cost;
        }

        // --- LOGIC TẠO GÓI SPIN ---
        public (SpinOption option1, SpinOption option2) GetSpinOptions(int currentRound, BigInteger currentCoin)
        {
            int baseCost = CalculateBaseSpinCost(currentRound);
            
            // Ép kiểu BigInt sang int để tính toán spin (vì giá spin không quá lớn đến mức tràn int)
            // Nếu coin quá lớn thì kẹp lại max int
            int coin = (currentCoin > int.MaxValue) ? int.MaxValue : (int)currentCoin;

            // Option 1: Nút bên trái (Thường là Standard hoặc Underspin)
            SpinOption opt1 = new SpinOption();
            
            // Option 2: Nút bên phải (Thường là Fewer Spins hoặc Disable)
            SpinOption opt2 = new SpinOption();

            int standardCost = baseCost * 7;
            int fewerCost = baseCost * 4;

            // CASE 1: ĐỦ TIỀN MUA GÓI CHUẨN (Standard)
            if (coin >= standardCost)
            {
                // Setup Option 1: Standard (7 Spins, +1 Ticket)
                opt1.type = SpinOptionType.Standard;
                opt1.title = "Standard Spins";
                opt1.spinCount = 7;
                opt1.coinCost = standardCost;
                opt1.ticketReward = 1;
                opt1.isAffordable = true;
                opt1.isAvailable = true;
                opt1.description = $"Cost: {standardCost} Coin\nGet 7 Spins.\nReward: +1 Ticket.";

                // Setup Option 2: Fewer Spins (4 Spins, +2 Ticket)
                opt2.type = SpinOptionType.FewerSpins;
                opt2.title = "Fewer Spins";
                opt2.spinCount = 4;
                opt2.coinCost = fewerCost; // Vẫn tốn tiền dựa trên số spin
                opt2.ticketReward = 2; // Thưởng nhiều vé hơn
                opt2.isAffordable = true;
                opt2.isAvailable = true;
                opt2.description = $"Cost: {fewerCost} Coin\nGet 4 Spins.\nReward: +2 Tickets!";
            }
            // CASE 2: KHÔNG ĐỦ 7 SPIN NHƯNG CÒN TIỀN (Underspin)
            else if (coin > 0)
            {
                // Tính số spin mua được (làm tròn xuống)
                int affordableSpins = coin / baseCost;

                // Nếu tiền lẻ quá không mua nổi 1 spin -> Chuyển sang Case Phá sản (Free)
                if (affordableSpins == 0)
                {
                    GenerateBankruptcyOptions(ref opt1, ref opt2);
                }
                else
                {
                    // Setup Option 1: Underspin
                    opt1.type = SpinOptionType.Underspin;
                    opt1.title = "Underspin";
                    opt1.spinCount = affordableSpins;
                    opt1.coinCost = affordableSpins * baseCost; // Trừ hết tiền chẵn
                    opt1.ticketReward = 1; // Vẫn cho 1 vé (theo logic game gốc)
                    opt1.isAffordable = true;
                    opt1.isAvailable = true;
                    opt1.description = $"Not enough for 7!\nSpend all to get {affordableSpins} Spins.\nReward: +1 Ticket.";

                    // Setup Option 2: Disable
                    opt2.type = SpinOptionType.FewerSpins;
                    opt2.title = "Unavailable";
                    opt2.isAvailable = false; // Khóa nút 2
                    opt2.description = "Not enough coins to choose this option.";
                }
            }
            // CASE 3: HẾT SẠCH TIỀN (Bankruptcy)
            else
            {
                GenerateBankruptcyOptions(ref opt1, ref opt2);
            }

            return (opt1, opt2);
        }

        private void GenerateBankruptcyOptions(ref SpinOption opt1, ref SpinOption opt2)
        {
            // Setup Option 1: Free Spin
            opt1.type = SpinOptionType.Free;
            opt1.title = "Bankruptcy";
            opt1.spinCount = 1;
            opt1.coinCost = 0;
            opt1.ticketReward = 1; 
            opt1.isAffordable = true;
            opt1.isAvailable = true;
            opt1.description = "You are broke!\nGet 1 Free Spin from the landlord.";

            // Setup Option 2: Disable
            opt2.type = SpinOptionType.FewerSpins;
            opt2.title = "Unavailable";
            opt2.isAvailable = false;
            opt2.description = "You have no coins.";
        }
    }
}