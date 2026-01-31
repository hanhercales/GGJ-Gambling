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
        // CÔNG THỨC CHUẨN: 4 THÀNH PHẦN
        // Thêm tham số activeCharms (có thể null) để tính các buff từ Charm
        public float GetScore(float globalSymMult, float globalPatMult, List<CharmData> activeCharms = null)
        {
            // 1. Symbol Value (Dynamic) - Bắt đầu từ giá trị gốc
            // Dùng long để tránh tràn số nếu sau này nhân quá lớn
            long processedSymbolVal = symbol.currentValue;
            
            // Nếu có charm, chạy qua từng charm để sửa đổi giá trị Symbol (VD: Chanh +1)
            if (activeCharms != null)
            {
                foreach (var charm in activeCharms)
                {
                    processedSymbolVal = charm.ModifySymbolScore(processedSymbolVal, symbol, symbol.idName);
                }
            }

            // 2. Pattern Value (Dynamic)
            float valPattern = pattern.currentMultiplier;

            // 3 & 4. Tính toán: (SymVal * GlobalSymMult) * (PatVal * GlobalPatMult)
            return (processedSymbolVal * globalSymMult) * (valPattern * globalPatMult);
        }

        public bool Contains(MatchResult other)
        {
            if (this.symbol != other.symbol) return false;
            foreach (var c in other.matchedCoordinates)
                if (!this.matchedCoordinates.Contains(c)) return false;
            return true;
        }
        #endregion
    }
}