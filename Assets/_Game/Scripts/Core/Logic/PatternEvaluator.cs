using System.Collections.Generic;
using UnityEngine;
using _Game.Scripts.Core.Data;
using System.Linq; 

namespace _Game.Scripts.Core.Logic
{
    public class PatternEvaluator
    {
        private List<PatternData> _allPatterns;

        // Constructor: Nhận data và sắp xếp ưu tiên ngay lập tức
        public PatternEvaluator(List<PatternData> patterns)
        {
            // SẮP XẾP: Priority cao trước -> Số ô nhiều trước
            _allPatterns = patterns
                .OrderByDescending(p => p.priority)
                .ThenByDescending(p => p.relativeCoordinates.Count)
                .ToList();
        }

        #region API Chính
        public List<MatchResult> Evaluate(SymbolData[,] grid, int cols, int rows)
        {
            // B1: Vét cạn tìm tất cả các match có thể (kể cả chồng lấn)
            List<MatchResult> rawMatches = FindAllMatches(grid, cols, rows);

            // B2: Lọc bỏ các match nhỏ nằm lọt thỏm trong match lớn
            return FilterSubsetMatches(rawMatches);
        }
        #endregion

        #region Logic Tìm kiếm & Lọc
        // Duyệt toàn bộ lưới để tìm pattern khớp
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
                            // Nếu khớp -> Tạo kết quả mới
                            MatchResult match = new MatchResult {
                                pattern = pattern,
                                symbol = matchedSymbol,
                                matchedCoordinates = new List<Vector2Int>()
                            };
                            
                            // Lưu lại tọa độ thực tế trên lưới
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

        // Loại bỏ các pattern con (Subset Filtering)
        private List<MatchResult> FilterSubsetMatches(List<MatchResult> rawMatches)
        {
            List<MatchResult> validMatches = new List<MatchResult>();
            
            // Sắp xếp danh sách thô: Thằng To xếp trước để xét duyệt trước
            var sortedRaw = rawMatches
                .OrderByDescending(m => m.pattern.priority)
                .ThenByDescending(m => m.matchedCoordinates.Count)
                .ToList();

            foreach (var candidate in sortedRaw)
            {
                bool isConsumed = false;

                // So sánh 'candidate' với các thằng khác trong list
                foreach (var existing in sortedRaw)
                {
                    if (candidate == existing) continue; // Bỏ qua chính nó

                    // Điều kiện bị nuốt: 
                    // 1. Thằng kia To hơn (Priority hoặc Size)
                    // 2. Thằng kia chứa trọn vẹn thằng này
                    bool isLarger = (existing.pattern.priority > candidate.pattern.priority) || 
                                    (existing.matchedCoordinates.Count > candidate.matchedCoordinates.Count);
                    
                    if (isLarger && existing.Contains(candidate))
                    {
                        // Ngoại lệ: Jackpot không nuốt thằng khác (nếu muốn)
                        if (existing.pattern.patternName != "JACKPOT") 
                        {
                            isConsumed = true;
                            break; // Bị nuốt rồi thì thôi, không cần check tiếp
                        }
                    }
                }

                // Nếu sống sót -> Thêm vào list hợp lệ
                if (!isConsumed) validMatches.Add(candidate);
            }
            return validMatches;
        }

        // Kiểm tra 1 pattern tại tọa độ gốc (startX, startY)
        private bool CheckMatchAt(SymbolData[,] grid, PatternData pattern, int startX, int startY, int cols, int rows, out SymbolData foundSymbol)
        {
            foundSymbol = null;
            SymbolData firstSymbol = null;

            foreach (var offset in pattern.relativeCoordinates)
            {
                int tx = startX + offset.x;
                int ty = startY + offset.y;

                // 1. Check biên
                if (tx < 0 || tx >= cols || ty < 0 || ty >= rows) return false;

                // 2. Check đồng nhất Symbol
                SymbolData current = grid[tx, ty];
                if (firstSymbol == null) firstSymbol = current; // Lấy ô đầu làm chuẩn
                else if (current != firstSymbol) return false;  // Khác chuẩn -> Fail
            }

            foundSymbol = firstSymbol;
            return true;
        }
        #endregion
    }
}