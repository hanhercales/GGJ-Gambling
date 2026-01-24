using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using System.Linq;

namespace _Game.Scripts.Core.Logic
{
    // Class chứa kết quả match
    [System.Serializable]
    public class MatchResult
    {
        public PatternData pattern;
        public SymbolData symbol;
        public List<Vector2Int> matchedCoordinates;

        public float GetScore() => symbol.baseValue * pattern.multiplier;

        // Kiểm tra xem match này có chứa trọn vẹn match kia không
        public bool Contains(MatchResult other)
        {
            if (this.symbol != other.symbol) return false;
            foreach (var c in other.matchedCoordinates)
                if (!this.matchedCoordinates.Contains(c)) return false;
            return true;
        }
    }

    public class PatternEvaluator
    {
        private List<PatternData> _allPatterns;

        public PatternEvaluator(List<PatternData> patterns)
        {
            // Sort: Priority cao & To hơn xếp trước
            _allPatterns = patterns
                .OrderByDescending(p => p.priority)
                .ThenByDescending(p => p.relativeCoordinates.Count)
                .ToList();
        }

        #region API Chính
        public List<MatchResult> Evaluate(SymbolData[,] grid, int cols, int rows)
        {
            // B1: Tìm tất cả match có thể (Vét cạn)
            List<MatchResult> rawMatches = FindAllMatches(grid, cols, rows);

            // B2: Lọc bỏ các match là tập con của match lớn hơn
            return FilterSubsetMatches(rawMatches);
        }
        #endregion

        #region Logic xử lý
        private List<MatchResult> FindAllMatches(SymbolData[,] grid, int cols, int rows)
        {
            List<MatchResult> matches = new List<MatchResult>();
            foreach (var pattern in _allPatterns)
            {
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        if (CheckMatchAt(grid, pattern, x, y, cols, rows, out SymbolData matchedSymbol))
                        {
                            MatchResult match = new MatchResult {
                                pattern = pattern,
                                symbol = matchedSymbol,
                                matchedCoordinates = new List<Vector2Int>()
                            };
                            foreach (var offset in pattern.relativeCoordinates)
                                match.matchedCoordinates.Add(new Vector2Int(x + offset.x, y + offset.y));
                            
                            matches.Add(match);
                        }
                    }
                }
            }
            return matches;
        }

        private List<MatchResult> FilterSubsetMatches(List<MatchResult> rawMatches)
        {
            List<MatchResult> validMatches = new List<MatchResult>();
            
            // Sắp xếp danh sách thô để ưu tiên thằng to
            var sortedRaw = rawMatches
                .OrderByDescending(m => m.pattern.priority)
                .ThenByDescending(m => m.matchedCoordinates.Count)
                .ToList();

            foreach (var candidate in sortedRaw)
            {
                bool isConsumed = false;
                foreach (var existing in sortedRaw)
                {
                    if (candidate == existing) continue;

                    // Nếu 'existing' to hơn VÀ chứa trọn vẹn 'candidate' -> 'candidate' bị nuốt
                    bool isLarger = (existing.pattern.priority > candidate.pattern.priority) || 
                                    (existing.matchedCoordinates.Count > candidate.matchedCoordinates.Count);
                    
                    if (isLarger && existing.Contains(candidate))
                    {
                        if (existing.pattern.patternName != "JACKPOT") // Jackpot không nuốt (tuỳ luật)
                        {
                            isConsumed = true;
                            break;
                        }
                    }
                }
                if (!isConsumed) validMatches.Add(candidate);
            }
            return validMatches;
        }

        // Kiểm tra khớp hình tại (startX, startY)
        private bool CheckMatchAt(SymbolData[,] grid, PatternData pattern, int startX, int startY, int cols, int rows, out SymbolData foundSymbol)
        {
            foundSymbol = null;
            SymbolData firstSymbol = null;

            foreach (var offset in pattern.relativeCoordinates)
            {
                int tx = startX + offset.x;
                int ty = startY + offset.y;

                if (tx < 0 || tx >= cols || ty < 0 || ty >= rows) return false; // Out bounds

                SymbolData current = grid[tx, ty];
                if (firstSymbol == null) firstSymbol = current;
                else if (current != firstSymbol) return false; // Khác loại
            }
            foundSymbol = firstSymbol;
            return true;
        }
        #endregion
    }
}