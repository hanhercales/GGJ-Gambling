using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Core.Data
{
    [CreateAssetMenu(fileName = "NewDifficultyProfile", menuName = "GameConfig/Debt Difficulty Profile")]
    public class DebtDifficultySO : ScriptableObject
    {
        [Header("Fixed Milestones")]
        [Tooltip("Danh sách nợ cố định cho từng vòng. Index 0 = Round 1.")]
        public List<int> milestones = new List<int>() 
        { 
            25,  // Round 1
            50,  // Round 2
            100, // Round 3
            225, // Round 4
            500, // Round 5
            1000 // Round 6
        };

        [Header("Endless Mode Config")]
        [Tooltip("Nếu hết mốc cố định, nợ sẽ nhân với số này.")]
        public float growthMultiplier = 1.5f;

        // Hàm tính toán chính - GameManager chỉ cần gọi hàm này
        public int GetDebtForRound(int round)
        {
            // Round trong game bắt đầu từ 1, nhưng List bắt đầu từ 0
            int index = round - 1;

            // TRƯỜNG HỢP 1: Vẫn nằm trong danh sách mốc cố định
            if (index >= 0 && index < milestones.Count)
            {
                return milestones[index];
            }

            // TRƯỜNG HỢP 2: Đã vượt quá danh sách -> Tính theo công thức lũy tiến
            // Lấy mốc cuối cùng làm gốc
            int lastFixedDebt = milestones[milestones.Count - 1];
            
            // Tính số vòng vượt quá
            int overflowRounds = round - milestones.Count;

            // Công thức: Nợ cuối * (1.5 ^ số vòng vượt)
            float calculatedDebt = lastFixedDebt * Mathf.Pow(growthMultiplier, overflowRounds);

            return Mathf.RoundToInt(calculatedDebt);
        }
    }
}