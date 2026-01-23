using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using System.Linq;

namespace _Game.Scripts.Core.Logic
{
    [System.Serializable]
    public class MatchResult
    {
        public PatternData pattern;
        public SymbolData symbol;
        public List<Vector2Int> matchedCoordinates;

        public float GetScore()
        {
            return symbol.baseValue * pattern.multiplier;
        }

        // HÀM MỚI: Kiểm tra xem match này có chứa hoàn toàn match kia không
        public bool Contains(MatchResult other)
        {
            // Nếu khác loại Symbol thì chắc chắn không chứa nhau
            if (this.symbol != other.symbol) return false;

            // Kiểm tra từng tọa độ của 'other' xem có nằm trong 'this' không
            foreach (var otherCoord in other.matchedCoordinates)
            {
                if (!this.matchedCoordinates.Contains(otherCoord))
                {
                    return false; // Có 1 ô lòi ra ngoài -> Không phải tập con
                }
            }
            return true; // Tất cả ô của other đều nằm trong this
        }
    }

    public class PatternEvaluator
    {
        private List<PatternData> _allPatterns;

        public PatternEvaluator(List<PatternData> patterns)
        {
            // Sắp xếp: Ưu tiên Priority cao, sau đó đến Kích thước lớn
            _allPatterns = patterns
                .OrderByDescending(p => p.priority)
                .ThenByDescending(p => p.relativeCoordinates.Count)
                .ToList();
        }

        public List<MatchResult> Evaluate(SymbolData[,] grid, int cols, int rows)
        {
            // BƯỚC 1: TÌM TẤT CẢ CÁC MATCH CÓ THỂ (Không quan tâm chồng lấn)
            List<MatchResult> rawMatches = FindAllMatches(grid, cols, rows);

            // BƯỚC 2: LỌC CÁC MATCH LÀ TẬP CON (Subset Filtering)
            List<MatchResult> finalMatches = FilterSubsetMatches(rawMatches);

            return finalMatches;
        }

        private List<MatchResult> FindAllMatches(SymbolData[,] grid, int cols, int rows)
        {
            List<MatchResult> matches = new List<MatchResult>();

            foreach (var pattern in _allPatterns)
            {
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        // Ở bước này KHÔNG DÙNG mảng isUsed nữa
                        // Cho phép quét trùng lặp thoải mái
                        if (CheckMatchAt(grid, pattern, x, y, cols, rows, out SymbolData matchedSymbol))
                        {
                            MatchResult match = new MatchResult();
                            match.pattern = pattern;
                            match.symbol = matchedSymbol;
                            match.matchedCoordinates = new List<Vector2Int>();

                            foreach (var offset in pattern.relativeCoordinates)
                            {
                                match.matchedCoordinates.Add(new Vector2Int(x + offset.x, y + offset.y));
                            }
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

            // Sắp xếp danh sách tìm được theo độ to/ưu tiên giảm dần
            // Để đảm bảo Pattern to luôn được check trước
            var sortedRaw = rawMatches
                .OrderByDescending(m => m.pattern.priority)
                .ThenByDescending(m => m.matchedCoordinates.Count)
                .ToList();

            foreach (var candidate in sortedRaw)
            {
                bool isConsumed = false;

                // So sánh candidate với tất cả các match ĐÃ ĐƯỢC CHẤP NHẬN trước đó
                // Hoặc so sánh với chính danh sách sortedRaw cũng được, nhưng tối ưu hơn là so với list to hơn
                foreach (var existing in sortedRaw)
                {
                    if (candidate == existing) continue; // Bỏ qua chính nó

                    // Quy tắc: Chỉ xét Pattern LỚN HƠN (Về Priority hoặc Size)
                    // Nếu 'existing' to hơn và chứa hoàn toàn 'candidate' -> Loại 'candidate'
                    bool isExistingLarger = (existing.pattern.priority > candidate.pattern.priority) || 
                                            (existing.matchedCoordinates.Count > candidate.matchedCoordinates.Count);

                    if (isExistingLarger && existing.Contains(candidate))
                    {
                        // Trừ trường hợp ngoại lệ: JACKPOT (Theo yêu cầu của bạn, Jackpot là ngoại lệ)
                        // Nếu existing là Jackpot thì tùy luật, nhưng ở đây ta cứ theo luật chung trước.
                        if (existing.pattern.patternName != "JACKPOT") 
                        {
                            isConsumed = true;
                            break;
                        }
                    }
                }

                // Nếu không bị thằng to nào nuốt -> Thêm vào danh sách hợp lệ
                if (!isConsumed)
                {
                    validMatches.Add(candidate);
                }
            }

            return validMatches;
        }

        private bool CheckMatchAt(SymbolData[,] grid, PatternData pattern, int startX, int startY, int cols, int rows, out SymbolData foundSymbol)
        {
            foundSymbol = null;
            SymbolData firstSymbol = null;

            foreach (var offset in pattern.relativeCoordinates)
            {
                int targetX = startX + offset.x;
                int targetY = startY + offset.y;

                if (targetX < 0 || targetX >= cols || targetY < 0 || targetY >= rows)
                    return false;

                // Bỏ đoạn check isUsed ở đây

                SymbolData currentSym = grid[targetX, targetY];

                if (firstSymbol == null)
                {
                    firstSymbol = currentSym;
                }
                else if (currentSym != firstSymbol)
                {
                    return false;
                }
            }

            foundSymbol = firstSymbol;
            return true;
        }
    }
}