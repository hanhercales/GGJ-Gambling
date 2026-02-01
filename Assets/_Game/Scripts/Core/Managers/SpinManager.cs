using UnityEngine;
using _Game.Scripts.Core.Data;
using _Game.Scripts.Core.Inventory; 
using System.Numerics;

namespace _Game.Scripts.Core.Managers
{
    public class SpinManager : MonoBehaviour
    {
        public static SpinManager Instance { get; private set; }
        
        // --- [NEW] COST MULTIPLIER ---
        private float _costMultiplier = 1.0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // --- [NEW] API FOR CHARMS ---
        public void SetCostMultiplier(float value)
        {
            _costMultiplier = value;
            Debug.Log($"[SpinManager] Cost Multiplier set to {_costMultiplier}x");
            
            // Refresh UI immediately if GameManager is present
            if (GameManager.Instance != null) 
                GameManager.Instance.RequestSpinOptionsUpdate();
        }

        // --- CÔNG THỨC TÍNH GIÁ ---
        public int CalculateBaseSpinCost(int currentDebtRound)
        {
            if (currentDebtRound <= 1) return 1; 

            int term1 = 2 * (currentDebtRound - 1);
            int term2 = 1 + ((currentDebtRound - 1) / 5);
            int cost = Mathf.Max(1, term1 * term2);
            
            return cost;
        }

        // --- LOGIC TẠO GÓI SPIN ---
        public (SpinOption option1, SpinOption option2) GetSpinOptions(int currentRound, BigInteger currentCoin)
        {
            // 1. Calculate Base Cost
            int rawBaseCost = CalculateBaseSpinCost(currentRound);
            
            // 2. [NEW] Apply Multiplier (e.g., Mask of Lust doubles this)
            // We verify it's at least 1 to avoid divide-by-zero later
            int finalBaseCost = Mathf.RoundToInt(rawBaseCost * _costMultiplier);
            if (finalBaseCost < 1) finalBaseCost = 1;

            int coin = (currentCoin > int.MaxValue) ? int.MaxValue : (int)currentCoin;

            SpinOption opt1 = new SpinOption();
            SpinOption opt2 = new SpinOption();

            int standardCost = finalBaseCost * 7;
            int fewerCost = finalBaseCost * 4;

            // CASE 1: ĐỦ TIỀN MUA GÓI CHUẨN
            if (coin >= standardCost)
            {
                opt1.type = SpinOptionType.Standard;
                opt1.title = "Standard Spins";
                opt1.spinCount += 7;
                opt1.coinCost = standardCost;
                opt1.ticketReward = 1;
                opt1.isAffordable = true;
                opt1.isAvailable = true;
                opt1.description = $"Cost: {standardCost} Coin\nGet 7 Spins.\nReward: +1 Ticket.";

                opt2.type = SpinOptionType.FewerSpins;
                opt2.title = "Fewer Spins";
                opt2.spinCount += 4;
                opt2.coinCost = fewerCost;
                opt2.ticketReward = 2;
                opt2.isAffordable = true;
                opt2.isAvailable = true;
                opt2.description = $"Cost: {fewerCost} Coin\nGet 4 Spins.\nReward: +2 Tickets!";
            }
            // CASE 2: KHÔNG ĐỦ 7 SPIN NHƯNG CÒN TIỀN
            else if (coin > 0)
            {
                // Note: finalBaseCost is higher now, so you get FEWER spins for the same money!
                int affordableSpins = coin / finalBaseCost;

                if (affordableSpins == 0)
                {
                    GenerateBankruptcyOptions(ref opt1, ref opt2);
                }
                else
                {
                    opt1.type = SpinOptionType.Underspin;
                    opt1.title = "Underspin";
                    opt1.spinCount = affordableSpins;
                    opt1.coinCost = affordableSpins * finalBaseCost;
                    opt1.ticketReward = 1;
                    opt1.isAffordable = true;
                    opt1.isAvailable = true;
                    opt1.description = $"Not enough for 7!\nSpend all to get {affordableSpins} Spins.\nReward: +1 Ticket.";

                    opt2.type = SpinOptionType.FewerSpins;
                    opt2.title = "Unavailable";
                    opt2.isAvailable = false;
                    opt2.description = "Not enough coins.";
                }
            }
            // CASE 3: HẾT SẠCH TIỀN
            else
            {
                GenerateBankruptcyOptions(ref opt1, ref opt2);
            }

            return (opt1, opt2);
        }

        private void GenerateBankruptcyOptions(ref SpinOption opt1, ref SpinOption opt2)
        {
            opt1.type = SpinOptionType.Free;
            opt1.title = "Bankruptcy";
            opt1.spinCount = 1;
            opt1.coinCost = 0;
            opt1.ticketReward = 1; 
            opt1.isAffordable = true;
            opt1.isAvailable = true;
            opt1.description = "You are broke!\nGet 1 Free Spin from the landlord.";

            opt2.type = SpinOptionType.FewerSpins;
            opt2.title = "Unavailable";
            opt2.isAvailable = false;
            opt2.description = "You have no coins.";
        }
    }
}