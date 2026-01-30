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
        public float GetScore(float globalSymMult, float globalPatMult)
        {
            // 1. Symbol Value (Dynamic)
            float valSymbol = symbol.currentValue;

            // 2. Pattern Value (Dynamic)
            float valPattern = pattern.currentMultiplier;

            // 3 & 4. Global Multipliers (truyền vào từ ScoreManager)
            
            // Tính toán: (SymVal * GlobalSymMult) * (PatVal * GlobalPatMult)
            return (valSymbol * globalSymMult) * (valPattern * globalPatMult);
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