using System.Collections.Generic;
using UnityEngine;
using System.Numerics;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewDifficultyProfile", menuName = "GameConfig/Debt Difficulty Profile")]
    public class DebtDifficultySO : ScriptableObject
    {
        [Header("Fixed Deadlines (Round 1-9)")]
        [Tooltip("Danh sách nợ cố định cho từng vòng. Index 0 = Round 1.")]
        public List<string> fixedDebtsInput = new List<string>()
        {
            "75",       // Round 1
            "200",      // Round 2
            "666",      // Round 3
            "2222",     // Round 4
            "12500",    // Round 5
            "33333",    // Round 6
            "66666",    // Round 7
            "200000",   // Round 8
            "1000000"   // Round 9
        };
        
        private List<BigInteger> _cachedFixedDebts;

        public BigInteger GetDebtForRound(int round)
        {
            // 1. Parse dữ liệu từ Inspector (Chỉ làm 1 lần)
            if (_cachedFixedDebts == null || _cachedFixedDebts.Count == 0)
            {
                _cachedFixedDebts = new List<BigInteger>();
                foreach (var s in fixedDebtsInput)
                {
                    // Nếu nhập sai định dạng thì mặc định là 0
                    if (BigInteger.TryParse(s, out BigInteger res)) _cachedFixedDebts.Add(res);
                    else _cachedFixedDebts.Add(0);
                }
            }

            // 2. Logic tính toán
            
            // GIAI ĐOẠN 1: FIXED (Round 1-9)
            if (round <= 9)
            {
                int index = round - 1;
                if (index < _cachedFixedDebts.Count)
                    return _cachedFixedDebts[index];
                
                return 1000000; // Fallback an toàn
            }
            
            // GIAI ĐOẠN 2: ENDLESS FORMULA (Round 10+)
            return CalculateEndlessDebt(round);
        }
        
        private BigInteger CalculateEndlessDebt(int round)
        {
            // Công thức chuẩn từ tài liệu:
            
            // 1. OverLimit = Current Deadline - 9
            int overLimit = round - 9;

            // 2. ScaleFactor = (OverLimit ^ Max(0, (OverLimit - 3)))
            int exponentScale = Mathf.Max(0, overLimit - 3);
            BigInteger scaleFactor = BigInteger.Pow(overLimit, exponentScale);

            // 3. ModOverLimit (Điều chỉnh nếu > 7)
            // if OverLimit > 7: OverLimit += OverLimit - 7
            int modOverLimit = overLimit;
            if (overLimit > 7)
            {
                modOverLimit += (overLimit - 7);
            }

            // 4. Base Debt = 1e6 * ((6 * 2 ^ (ModOverLimit - 1)) ^ ModOverLimit) * ScaleFactor
            
            // a. term1 = 6 * 2^(ModOverLimit - 1)
            BigInteger twoPow = BigInteger.Pow(2, modOverLimit - 1);
            BigInteger term1 = 6 * twoPow;

            // b. term2 = term1 ^ ModOverLimit
            BigInteger term2 = BigInteger.Pow(term1, modOverLimit);

            // c. Kết quả cuối cùng
            BigInteger result = 1000000 * term2 * scaleFactor;

            return result;
        }
    }
}