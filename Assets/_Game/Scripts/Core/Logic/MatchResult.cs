using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;

namespace _Game.Scripts.Core.Logic
{
    [System.Serializable]
    public class MatchResult
    {
        #region Dữ liệu Match
        public PatternData pattern;                 
        public SymbolData symbol;                  
        public List<Vector2Int> matchedCoordinates; 
        #endregion

        #region Logic Phụ trợ
        // Tính điểm nhanh: Giá trị Symbol * Hệ số Pattern
        public float GetScore() => symbol.baseValue * pattern.multiplier;

        // Kiểm tra xem Match này có chứa trọn vẹn Match kia không? (Dùng để lọc tập con)
        public bool Contains(MatchResult other)
        {
            // Khác loại trái cây -> Chắc chắn không chứa
            if (this.symbol != other.symbol) return false;

            // Kiểm tra từng tọa độ: Nếu có 1 ô lòi ra ngoài -> Không chứa
            foreach (var c in other.matchedCoordinates)
            {
                if (!this.matchedCoordinates.Contains(c)) return false;
            }
            return true; // Chứa hoàn toàn
        }
        #endregion
    }
}